namespace CRMService.Application.Abstractions.Service
{
    public interface INotificationService
    {
        Task SendMessage(long? chatId, string content, CancellationToken ct = default);
    }
}



