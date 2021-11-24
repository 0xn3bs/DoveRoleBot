namespace DoveRoleBot
{
    public class BotSettings
    {
        public int UpdateInterval { get; set; } = 30;
        public string BotToken { get; set; } = null!;
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public int TimeLimit { get; set; }
        public ulong KickChannelId { get; set; }
    }
}