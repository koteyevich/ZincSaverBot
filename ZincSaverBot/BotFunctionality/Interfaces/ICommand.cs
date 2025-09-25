using Telegram.Bot.Types;

namespace ZincSaverBot.BotFunctionality.Interfaces;

public interface ICommand
{
    BotCommand BotCommand { get; }
    string[] Aliases { get; }
    int Order { get; }
    Task ExecuteAsync(Message message);
}