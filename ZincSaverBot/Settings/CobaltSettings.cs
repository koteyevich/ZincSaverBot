namespace ZincSaverBot.Settings;

public interface ICobaltSettings
{
    string ApiKey { get; set; }
    string Url { get; set; }
    string UserAgent { get; set; }
}

public class CobaltSettings : ICobaltSettings
{
    public string ApiKey { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string UserAgent { get; set; } = null!;
}