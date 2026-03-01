namespace CRMService.Application.Models.ConfigClass
{
    public class TelegramBotOptions
    {
        public const string SectionName = "TelegramBot";
        public long SupportChatId { get; set; }
        public long DebugChatId { get; set; }
    }
}



