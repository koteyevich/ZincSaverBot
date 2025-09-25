using Telegram.Bot.Types;

namespace ZincSaverBot.Settings;

public interface IBotInfo
{
    User Me { get; set; }
}

public class BotInfo : IBotInfo
{
    public User Me { get; set; } = null!;
}