using GithubSync.Application.Github;
using GithubSync.Application.Issues;
using GithubSync.Application.Sync;
using GithubSync.Infrastructure;
using GithubSync.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<GithubOptions>(builder.Configuration.GetSection("GitHub"));

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("AppDb"));
});

builder.Services.AddHttpClient<IGithubClient, GithubClient>()
    .ConfigureHttpClient(http =>
    {
        http.BaseAddress = new Uri("https://api.github.com");
        http.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(GithubRetryPolicy.GetGitHubRetryPolicy());

builder.Services.AddHttpClient<GitHubReachabilityHealthCheck>(http =>
{
    http.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("db", failureStatus: HealthStatus.Unhealthy, tags: ["ready"])
    .AddCheck<GitHubReachabilityHealthCheck>("github", failureStatus: HealthStatus.Degraded, tags: ["ready"]);


builder.Services.AddScoped<ISyncStateRepository, EFSyncStateRepository>();
builder.Services.AddScoped<IIssueRepository, EFIssueRepository>();

builder.Services.AddScoped<SyncIssuesUseCase>();
builder.Services.AddScoped<ISyncIssues, OptionsBoundSyncIssues>();

builder.Services.AddSingleton<GithubSync.Infrastructure.Sync.SingleSyncGate>();
builder.Services.AddHostedService<GithubSync.Infrastructure.Sync.ScheduledSyncService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});


var app = builder.Build();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GithubSync v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();



// Required for unit testing
public partial class Program { }
