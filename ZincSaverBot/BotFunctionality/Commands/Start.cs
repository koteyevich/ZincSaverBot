using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ZincSaverBot.BotFunctionality.Interfaces;
using ZincSaverBot.Helpers;

namespace ZincSaverBot.BotFunctionality.Commands;

public class Start(ITelegramBotClient bot) : ICommand
{
    public BotCommand BotCommand => new("/start", "Start a bot.");

    public string[] Aliases => [];
    public int Order => 1;

    public async Task ExecuteAsync(Message message)
    {
        await bot.SendMessage(message,
            "Hi! \n" +
            "Send a link to a TikTok video or X (formerly known as Twitter) to download media!");
    }
}