using Telegram.Bot.Types;

namespace ZincSaverBot.BotFunctionality.Interfaces;

public interface ICallback
{
    string Name { get; }

    Task ExecuteAsync(CallbackQuery callbackQuery);
}