namespace CRMService.Models.ConfigClass
{
    public class TelegramBotOptions
    {
        public const string TELEGRAM_BOT = "TelegramBot";
        public long SupportChatId { get; set; }
        public long DebugChatId { get; set; }
    }
}
