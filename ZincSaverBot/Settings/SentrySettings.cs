using Microsoft.Extensions.Logging;

namespace ZincSaverBot.Settings;

public class SentrySettings
{
    public required string Dsn { get; set; }
    public LogLevel MinimumBreadcrumbLevel { get; set; }
    public LogLevel MinimumEventLevel { get; set; }
    public int MaxBreadcrumbs { get; set; }
}