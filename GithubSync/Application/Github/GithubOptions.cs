namespace GithubSync.Application.Github
{
    public class GithubOptions
    {
        public required string Repository { get; init; }
        public required string Token { get; init; }
        public int SyncIntervalMinutes { get; init; } = 10;
    }
}
