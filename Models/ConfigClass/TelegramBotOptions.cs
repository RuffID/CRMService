namespace CRMService.Models.ConfigClass
{
    public class TelegramBotOptions
    {
        public const string SectionName = "TelegramBot";
        public long ChatId { get; set; }
        public long DebugChatId { get; set; }
    }
}
