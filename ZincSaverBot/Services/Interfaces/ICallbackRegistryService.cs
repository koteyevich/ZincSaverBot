using Telegram.Bot.Types;

namespace ZincSaverBot.Services.Interfaces;

public interface ICallbackRegistryService
{
    Task HandleCallbackAsync(CallbackQuery callbackQuery);
}