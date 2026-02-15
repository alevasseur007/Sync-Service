using GithubSync.Application.Github;
using GithubSync.Infrastructure.Persistence;
using GithubSync.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace GithubSync.Tests
{
    public sealed class TestAppFactory : WebApplicationFactory<Program>
    {
        private readonly IReadOnlyList<GithubIssueDTO> _seedIssues;
        private DbConnection? _connection;

        public TestAppFactory(IReadOnlyList<GithubIssueDTO> seedIssues)
            => _seedIssues = seedIssues;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbContextDescriptor is not null)
                    services.Remove(dbContextDescriptor);

                // Create an in-memory SQLite connection that stays open for the test lifetime
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlite(_connection));

                var githubDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGithubClient));
                if (githubDescriptor is not null)
                    services.Remove(githubDescriptor);

                services.AddSingleton<IGithubClient>(new MockGithubClient(_seedIssues));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Dispose();
        }
    }
}
