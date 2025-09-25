using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ZincSaverBot.Helpers;

public static class TelegramBotClientExtensions
{
    /// <summary>Method to send text messages in the same messageThreadId and chat id (chat id can still me explicitly specified)</summary>
    /// <param name="botClient">An instance of <see cref="ITelegramBotClient"/></param>
    /// <param name="message">Message instance.</param>
    /// <param name="text">Text of the message to be sent, 1-4096 characters after entities parsing</param>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="parseMode">Mode for parsing entities in the message text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="linkPreviewOptions">Link preview generation options for the message</param>
    /// <param name="entities">A list of special entities that appear in message text, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="messageThreadId">Thread Id</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public static async Task<Message> SendMessage(this ITelegramBotClient botClient,
        Message message,
        string text,
        ChatId? chatId = null,
        ParseMode parseMode = default,
        ReplyParameters? replyParameters = null,
        ReplyMarkup? replyMarkup = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        IEnumerable<MessageEntity>? entities = null,
        int? messageThreadId = null,
        bool disableNotification = false,
        bool protectContent = false,
        string? messageEffectId = null,
        string? businessConnectionId = null,
        bool allowPaidBroadcast = false,
        CancellationToken cancellationToken = default)
    {
        return await botClient.SendRequest(new SendMessageRequest
        {
            Text = text,
            ParseMode = parseMode,
            ChatId = chatId ?? message.Chat.Id,
            ReplyParameters = replyParameters,
            ReplyMarkup = replyMarkup,
            LinkPreviewOptions = linkPreviewOptions,
            MessageThreadId = messageThreadId ?? message.MessageThreadId,
            Entities = entities,
            DisableNotification = disableNotification,
            ProtectContent = protectContent,
            MessageEffectId = messageEffectId,
            BusinessConnectionId = businessConnectionId,
            AllowPaidBroadcast = allowPaidBroadcast,
        }, cancellationToken);
    }

    /// <summary>Use this method to send animation files (GIF or H.264/MPEG-4 AVC video without sound).</summary>
    /// <remarks>Bots can currently send animation files of up to 50 MB in size, this limit may be changed in the future.</remarks>
    /// <param name="botClient">An instance of <see cref="ITelegramBotClient"/></param>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="animation">Animation to send. Pass a FileId as String to send an animation that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get an animation from the Internet, or upload a new animation using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Animation caption (may also be used when resending animation by <em>FileId</em>), 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the animation caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="duration">Duration of sent animation in seconds</param>
    /// <param name="width">Animation width</param>
    /// <param name="height">Animation height</param>
    /// <param name="thumbnail">Thumbnail of the file sent; can be ignored if thumbnail generation for the file is supported server-side. The thumbnail should be in JPEG format and less than 200 kB in size. A thumbnail's width and height should not exceed 320. Ignored if the file is not uploaded using <see cref="InputFileStream"/>. Thumbnails can't be reused and can be only uploaded as a new file, so you can use <see cref="InputFileStream(Stream, string?)"/> with a specific filename. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="showCaptionAboveMedia">Pass <see langword="true"/>, if the caption must be shown above the message media</param>
    /// <param name="hasSpoiler">Pass <see langword="true"/> if the animation needs to be covered with a spoiler animation</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="directMessagesTopicId">Identifier of the direct messages topic to which the message will be sent; required if the message is sent to a direct messages chat</param>
    /// <param name="suggestedPostParameters">An object containing the parameters of the suggested post to send; for direct messages chats only. If the message is sent as a reply to another suggested post, then that suggested post is automatically declined.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public static async Task<Message> SendAnimation(
        this ITelegramBotClient botClient,
        Message message,
        InputFile animation,
        ChatId? chatId = null,
        string? caption = null,
        ParseMode parseMode = default,
        ReplyParameters? replyParameters = null,
        ReplyMarkup? replyMarkup = null,
        int? duration = null,
        int? width = null,
        int? height = null,
        InputFile? thumbnail = null,
        int? messageThreadId = null,
        IEnumerable<MessageEntity>? captionEntities = null,
        bool showCaptionAboveMedia = false,
        bool hasSpoiler = false,
        bool disableNotification = false,
        bool protectContent = false,
        string? messageEffectId = null,
        string? businessConnectionId = null,
        bool allowPaidBroadcast = false,
        long? directMessagesTopicId = null,
        SuggestedPostParameters? suggestedPostParameters = null,
        CancellationToken cancellationToken = default
    )
    {
        return await botClient.SendRequest(new SendAnimationRequest
        {
            ChatId = chatId ?? message.Chat.Id,
            Animation = animation,
            Caption = caption,
            ParseMode = parseMode,
            ReplyParameters = replyParameters,
            ReplyMarkup = replyMarkup,
            Duration = duration,
            Width = width,
            Height = height,
            Thumbnail = thumbnail,
            MessageThreadId = messageThreadId ?? message.MessageThreadId,
            CaptionEntities = captionEntities,
            ShowCaptionAboveMedia = showCaptionAboveMedia,
            HasSpoiler = hasSpoiler,
            DisableNotification = disableNotification,
            ProtectContent = protectContent,
            MessageEffectId = messageEffectId,
            BusinessConnectionId = businessConnectionId,
            AllowPaidBroadcast = allowPaidBroadcast,
            DirectMessagesTopicId = directMessagesTopicId,
            SuggestedPostParameters = suggestedPostParameters
        }, cancellationToken).ConfigureAwait(false);
    }
}