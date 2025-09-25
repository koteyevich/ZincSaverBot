using Telegram.Bot.Types;

namespace ZincSaverBot.Services.Interfaces;

public interface ICommandRegistryService
{
    Task HandleCommand(Message message);
}