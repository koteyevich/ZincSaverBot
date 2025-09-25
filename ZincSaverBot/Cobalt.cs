using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZincSaverBot.Helpers;

namespace ZincSaverBot;

public record MediaItem(IAlbumInputMedia? Media, string Url);

public class Cobalt(CobaltServices cobaltServices, ILogger<Cobalt> logger, ITelegramBotClient bot)
{
    public async Task Download(Message message)
    {
        var urls = new List<string>();
        foreach (var url in message.Text.Split(' '))
        {
            logger.LogInformation("{link}", url);
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) continue;

            var builder = new UriBuilder(uri)
            {
                Fragment = string.Empty
            };
            urls.Add(builder.Uri.ToString());
        }

        var mediaGroup = new List<MediaItem>();
        foreach (var url in urls)
        {
            var response = await cobaltServices.Download(url);
            switch (response)
            {
                case Error error:
                    throw new ApiRequestException(error.Code);
                case TunnelRedirectResponse tunnelRedirectResponse:
                {
                    var mediaType = Path.GetExtension(tunnelRedirectResponse.Filename);
                    var file = new InputFileUrl(tunnelRedirectResponse.Url);

                    IAlbumInputMedia? media = mediaType switch
                    {
                        ".opus" or ".mp3" or ".ogg" => new InputMediaAudio(file),
                        ".mp4" or ".webm" => new InputMediaVideo(file),
                        ".png" or ".jpg" or ".jpeg" or ".webp" => new InputMediaPhoto(file),
                        _ => null
                    };

                    if (mediaType == ".gif")
                    {
                        await bot.SendAnimation(message, file);
                        break;
                    }

                    if (media != null)
                        mediaGroup.Add(new MediaItem(media, tunnelRedirectResponse.Url));


                    break;
                }
                case LocalProcessingResponse localProcessingResponse:
                {
                    var outputFile = localProcessingResponse.Output;

                    var mediaType = Path.GetExtension(outputFile.Filename).ToLowerInvariant();

                    var file = new InputFileUrl(localProcessingResponse.Tunnel.FirstOrDefault() ??
                                                outputFile.Filename);

                    IAlbumInputMedia? media = mediaType switch
                    {
                        ".opus" or ".mp3" or ".ogg" => new InputMediaAudio(file),
                        ".mp4" or ".webm" => new InputMediaVideo(file),
                        ".png" or ".jpg" or ".jpeg" or ".webp" => new InputMediaPhoto(file),
                        _ => null
                    };

                    if (mediaType == ".gif")
                    {
                        await bot.SendAnimation(message, file);
                        break;
                    }

                    if (media != null)
                        mediaGroup.Add(new MediaItem(media, file.Url.ToString()));

                    break;
                }

                case PickerResponse pickerResponse:
                {
                    foreach (var pickerObject in pickerResponse.Picker)
                    {
                        var file = new InputFileUrl(pickerObject.Url);

                        IAlbumInputMedia? media = pickerObject.Type switch
                        {
                            "photo" => new InputMediaPhoto(file),
                            "video" => new InputMediaVideo(file),
                            _ => null
                        };

                        if (pickerObject.Type == "gif")
                        {
                            await bot.SendAnimation(message, file);
                            break;
                        }

                        if (media != null)
                            mediaGroup.Add(new MediaItem(media, file.Url.ToString()));
                    }

                    break;
                }

                default:
                    throw new ApiRequestException("error.bot.unknown");
            }
        }

        const int chunkSize = 10;
        for (var i = 0; i < mediaGroup.Count; i += chunkSize)
        {
            var chunk = mediaGroup.Skip(i).Take(chunkSize).ToList();
            await bot.SendChatAction(message.Chat.Id, ChatAction.UploadDocument);

            try
            {
                await bot.SendMediaGroup(
                    chatId: message.Chat.Id,
                    media: chunk.Select(x => x.Media)
                );
            }
            catch (ApiRequestException ex) when (
                ex.Message.Contains("WEBPAGE_CURL_FAILED") ||
                ex.Message.Contains("file is too big")
            )
            {
                foreach (var item in chunk)
                {
                    switch (item.Media)
                    {
                        case InputMediaPhoto:
                            await bot.SendMessage(message, $"Photo too large: {item.Url}");
                            break;
                        case InputMediaVideo:
                            await bot.SendMessage(message, $"Video too large: {item.Url}");
                            break;
                        case InputMediaAudio:
                            await bot.SendMessage(message, $"Audio too large: {item.Url}");
                            break;
                        default:
                            await bot.SendMessage(message, $"File too large: {item.Url}");
                            break;
                    }
                }

                logger.LogWarning("Telegram rejected media group: {error}", ex.Message);
            }
        }
    }
}