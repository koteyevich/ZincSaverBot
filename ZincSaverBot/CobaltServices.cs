using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Exceptions;
using ZincSaverBot.Settings;

namespace ZincSaverBot;

public interface IResponseObject;

public class TunnelRedirectResponse : IResponseObject
{
    public string Status { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Filename { get; set; } = null!;
}

public class PickerObject
{
    public string Type { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Thumb { get; set; } = null!;
}

public class PickerResponse : IResponseObject
{
    public string Status { get; set; } = null!;
    public string Audio { get; set; } = null!;
    public string AudioFilename { get; set; } = null!;
    public List<PickerObject> Picker { get; set; } = null!;
}

public class LocalProcessingResponse : IResponseObject
{
    public string Status { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<string> Tunnel { get; set; } = new();
    public OutputFile Output { get; set; } = null!;
}

public class OutputFile
{
    public string Type { get; set; } = null!;
    public string Filename { get; set; } = null!;
}

public class Error : IResponseObject
{
    public string Code { get; set; } = null!;
}

public class CobaltServices
{
    private readonly IConfiguration _configuration;
    private CobaltSettings _cobaltSettings;

    public CobaltServices(IConfiguration configuration)
    {
        _configuration = configuration;
        _cobaltSettings = _configuration.GetRequiredSection(nameof(CobaltSettings)).Get<CobaltSettings>()!;
    }

    private static readonly HashSet<string> SupportedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "bilibili.com",
        "bsky.app",
        "dailymotion.com",
        "facebook.com",
        "instagram.com",
        "loom.com",
        "ok.ru",
        "pinterest.com",
        "newgrounds.com",
        "reddit.com",
        "rutube.ru",
        "snapchat.com",
        "soundcloud.com",
        "streamable.com",
        "tiktok.com",
        "tumblr.com",
        "twitch.tv",
        "twitter.com",
        "x.com",
        "vimeo.com",
        "vk.com",
        "xiaohongshu.com"
    };

    public async Task<IResponseObject> Download(string url, string downloadMode = "auto")
    {
        if (!IsSupportedUrl(url))
        {
            throw new NotSupportedException("Url is not supported"); // do not proceed.
        }

        var headers = new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" },
            { "User-Agent", $"{_cobaltSettings.UserAgent}" }
        };

        if (!string.IsNullOrWhiteSpace(_cobaltSettings.ApiKey))
        {
            headers.Add("Authorization", $"Api-Key {_cobaltSettings.ApiKey}");
        }

        var body = new Dictionary<string, string>
        {
            { "url", url },
            { "audioFormat", "mp3" },
            { "downloadMode", downloadMode },
            { "filenameStyle", "nerdy" }
        };

        using var client = new HttpClient();

        client.BaseAddress = new Uri(_cobaltSettings.Url);

        foreach (var header in headers.Where(header =>
                     !header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");


        var response = await client.PostAsync(_cobaltSettings.Url, content);
        response.EnsureSuccessStatusCode();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

        if (jsonResponse.ValueKind != JsonValueKind.Object)
        {
            return new Error { Code = "error.bot.invalid_response" };
        }

        if (!jsonResponse.TryGetProperty("status", out var statusElement) ||
            statusElement.ValueKind != JsonValueKind.String)
        {
            return new Error { Code = "error.bot.missing_status" };
        }

        var status = statusElement.GetString();
        switch (status)
        {
            case "error":
                if (jsonResponse.TryGetProperty("error", out var errorElement) &&
                    errorElement.ValueKind == JsonValueKind.Object &&
                    errorElement.TryGetProperty("code", out var codeElement) &&
                    codeElement.ValueKind == JsonValueKind.String)
                {
                    return new Error { Code = codeElement.GetString()! };
                }

                return new Error { Code = "error.bot.invalid_error_format" };
            case "tunnel" or "redirect":
                if (jsonResponse.TryGetProperty("url", out var urlElement) &&
                    jsonResponse.TryGetProperty("filename", out var filenameElement) &&
                    urlElement.ValueKind == JsonValueKind.String &&
                    filenameElement.ValueKind == JsonValueKind.String)
                {
                    return new TunnelRedirectResponse
                    {
                        Status = status,
                        Url = urlElement.GetString()!,
                        Filename = filenameElement.GetString()!
                    };
                }

                return new Error { Code = "error.bot.invalid_tunnel_redirect_format" };
            case "local-processing":
            {
                if (!jsonResponse.TryGetProperty("output", out var outputElement) ||
                    outputElement.ValueKind != JsonValueKind.Object)
                {
                    return new Error { Code = "error.bot.missing_output" };
                }

                var output = new OutputFile
                {
                    Type = outputElement.TryGetProperty("type", out var typeElement) &&
                           typeElement.ValueKind == JsonValueKind.String
                        ? typeElement.GetString()!
                        : string.Empty,
                    Filename = outputElement.TryGetProperty("filename", out var filenameElement1) &&
                               filenameElement1.ValueKind == JsonValueKind.String
                        ? filenameElement1.GetString()!
                        : string.Empty
                };

                var tunnel = new List<string>();
                if (jsonResponse.TryGetProperty("tunnel", out var tunnelElement) &&
                    tunnelElement.ValueKind == JsonValueKind.Array)
                {
                    tunnel.AddRange(from item in tunnelElement.EnumerateArray()
                        where item.ValueKind == JsonValueKind.String
                        select item.GetString()!);
                }

                return new LocalProcessingResponse
                {
                    Status = status,
                    Type = jsonResponse.TryGetProperty("type", out var typeEl) &&
                           typeEl.ValueKind == JsonValueKind.String
                        ? typeEl.GetString()!
                        : string.Empty,
                    Tunnel = tunnel,
                    Output = output
                };
            }
            case "picker":
                var mediaList = new List<PickerObject>();
                if (jsonResponse.TryGetProperty("picker", out var pickerElement) &&
                    pickerElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in pickerElement.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object ||
                            !item.TryGetProperty("type", out var typeElement) ||
                            !item.TryGetProperty("url", out var pickerUrlElement) ||
                            typeElement.ValueKind != JsonValueKind.String ||
                            pickerUrlElement.ValueKind != JsonValueKind.String) continue;

                        var pickerObject = new PickerObject
                        {
                            Type = typeElement.GetString()!,
                            Url = pickerUrlElement.GetString()!,
                            Thumb = item.TryGetProperty("thumb", out var thumbElement) &&
                                    thumbElement.ValueKind == JsonValueKind.String
                                ? thumbElement.GetString()!
                                : string.Empty
                        };
                        mediaList.Add(pickerObject);
                    }
                }

                return new PickerResponse
                {
                    Status = status,
                    Audio = jsonResponse.TryGetProperty("audio", out var audioElement) &&
                            audioElement.ValueKind == JsonValueKind.String
                        ? audioElement.GetString()!
                        : string.Empty,
                    AudioFilename = jsonResponse.TryGetProperty("audioFilename", out var audioFilenameElement) &&
                                    audioFilenameElement.ValueKind == JsonValueKind.String
                        ? audioFilenameElement.GetString()!
                        : string.Empty,
                    Picker = mediaList
                };
            default:
                throw new ApiRequestException("error.bot.unknown");
        }
    }

    public bool IsSupportedUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            string host = uri.Host.ToLowerInvariant();
            return SupportedDomains.Any(domain => host == domain || host.EndsWith("." + domain));
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
}