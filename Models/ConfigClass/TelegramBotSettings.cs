namespace CRMService.Models.ConfigClass
{
    public class TelegramBotSettings
    {
        public const string TELEGRAM_BOT = "TelegramBot";
        public long SupportChatId { get; set; }
        public long DebugChatId { get; set; }
        public bool DebugMode { get; set; }
    }
}
