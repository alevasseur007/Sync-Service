using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace GithubSync.Infrastructure
{
    public class GithubRetryPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetGitHubRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // 5xx, 408, network failures
                .OrResult(msg => msg.StatusCode == (HttpStatusCode)429) // Too Many Requests
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s,4s,8s
                    onRetry: (outcome, delay, attempt, _) => { /* Minimal for now */ });
        }
    }
}
