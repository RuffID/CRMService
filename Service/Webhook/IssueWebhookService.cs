using CRMService.API;
using CRMService.Interfaces.Api;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.WebHook;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Webhook
{
    public class IssueWebhookService(IUnitOfWork unitOfWork, IHttpApiClient client, IOptions<ApiEndpointOptions> endp,
        IOptions<TelegramBotOptions> tgSettings, ILoggerFactory logger) : IWebhookHandler
    {
        private const string AUTHOR_CONTACT_TYPE = "contact";
        private readonly ILogger<IssueWebhookService> _logger = logger.CreateLogger<IssueWebhookService>();
        private readonly TelegramNotification tgNotif = new(client, endp.Value);

        public async Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Issue == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_ticket_status":
                    await UpdateStatusAndSaveTimeEntries(@event, ct);
                    break;
                case "new_ticket":
                    await CreateIssue(@event.Issue, ct);
                    break;
                case "new_assignee":
                    await UpdateIssue(@event.Issue, ct);
                    break;
                case "update_issue_work_type":
                    await UpdateIssue(@event.Issue, ct);
                    break;
                case "ticket_deleted":
                    await MarkIssueAsDeletedAsync(@event.Issue, ct);
                    break;
                case "new_comment":
                    await NewComment(@event, ct);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private async Task CreateIssue(IssueWebHook issueJson, CancellationToken ct)
        {
            Issue issue = issueJson.ConvertToIssue();

            await unitOfWork.Issue.Upsert(issue, ct);

            _logger.LogInformation("[Method:{MethodName}] Create issue from webhook: Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(CreateIssue), issueJson.Id, issueJson.Status?.Code, issueJson.Priority?.Code, issueJson.Type?.Code, issueJson.Client?.Company?.Id, issueJson.Maintenance_entity?.Id, issueJson.Assignee?.Employee?.Id);

            await unitOfWork.SaveAsync(ct);

            string content = string.Empty;

            content += Priority(issueJson.Priority?.Code?.ToLower());
            content += $" {issue.Title}" + Environment.NewLine;
            content += $"{issueJson.Client?.Company?.Name}" + Environment.NewLine + Environment.NewLine;
            content += $"{endp.Value.OkdeskDomainUrl}/issues/{issue.Id}";

            if (issueJson.Author?.Type == AUTHOR_CONTACT_TYPE)
                await tgNotif.SendMessage(tgSettings.Value.ChatId, content, ct);
        }

        private async Task UpdateStatusAndSaveTimeEntries(RootEventWebHook @event, CancellationToken ct)
        {
            Issue issue = @event.Issue!.ConvertToIssue();

            _logger.LogInformation("[Method:{MethodName}] Update issue from webhook: \"{WebhookType}\". Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(UpdateStatusAndSaveTimeEntries), @event.Event!.Event_type, @event.Issue.Id, @event.Issue.Status?.Code, @event.Issue.Priority?.Code, @event.Issue.Type?.Code, @event.Issue.Client?.Company?.Id, @event.Issue.Maintenance_entity?.Id, @event.Issue.Assignee?.Employee?.Id);

            await unitOfWork.Issue.Upsert(issue, ct);

            List<TimeEntry> entries = new ();
            if (@event.Event!.Time_entries != null && @event.Event!.Time_entries.Length != 0)
            {
                foreach (TimeEntryWebHook entry in @event.Event.Time_entries)
                {
                    entries.Add(new()
                    {
                        Id = entry.Id,
                        SpentTime = entry.Spent_time,
                        EmployeeId = entry.Employee.Id,
                        IssueId = issue.Id,
                        CreatedAt = DateTime.Now,
                        LoggedAt = entry.Logged_at
                    });
                }

                await unitOfWork.TimeEntry.Upsert(entries, ct);

                _logger.LogInformation("[Method:{MethodName}] Create time entry from webhook: \"{WebhookType}\". Time entries count: {timeEntriesCount}, issueId: {issueId}, assigneeId: {assigneeId}",
                nameof(UpdateStatusAndSaveTimeEntries), @event.Event!.Event_type, entries.Count, @event.Issue.Id, @event.Issue.Assignee?.Employee?.Id);
            }
            
            await unitOfWork.SaveAsync(ct);
        }

        private async Task UpdateIssue(IssueWebHook issueJson, CancellationToken ct)
        {
            Issue issue = issueJson.ConvertToIssue();
            await unitOfWork.Issue.Upsert(issue, ct);

            _logger.LogInformation("[Method:{MethodName}] Update issue from webhook: Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(UpdateIssue), issueJson.Id, issueJson.Status?.Code, issueJson.Priority?.Code, issueJson.Type?.Code, issueJson.Client?.Company?.Id, issueJson.Maintenance_entity?.Id, issueJson.Assignee?.Employee?.Id);

            await unitOfWork.SaveAsync(ct);
        }

        private async Task MarkIssueAsDeletedAsync(IssueWebHook issueJson, CancellationToken ct)
        {
            Issue convertIssue = issueJson.ConvertToIssue();

            convertIssue.DeletedAt = DateTime.Now;
            await unitOfWork.Issue.Upsert(convertIssue, ct);

            _logger.LogInformation("[Method:{MethodName}] Delete issue from webhook: Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, old work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(MarkIssueAsDeletedAsync), issueJson.Id, issueJson.Status?.Code, issueJson.Priority?.Code, issueJson.Type?.Code, issueJson.Client?.Company?.Id, issueJson.Maintenance_entity?.Id, issueJson.Assignee?.Employee?.Id);

            await unitOfWork.SaveAsync(ct);
        }

        private async Task NewComment(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Event?.Author?.Type != AUTHOR_CONTACT_TYPE || string.IsNullOrEmpty(@event.Event?.Comment?.Content))
                return;

            DateTime now = DateTime.Now;
            DateTime evening = new (now.Year, now.Month, now.Day, hour: 18, minute: 0, second: 0);
            DateTime morning = new(now.Year, now.Month, now.Day, hour: 9, minute: 0, second: 0);

            // Не уведомлять, если сейчас между 09:00 и 18:00
            if (DateTime.Now > morning && DateTime.Now < evening)
                return;

            // Не уведомлять при объединении заявок
            if (@event.Event?.Comment?.Content.Contains("Комментарий добавлен при объединении заявок", StringComparison.CurrentCultureIgnoreCase) == true)
                return;

            string content = string.Empty;
            content += Priority(@event.Issue?.Priority.Code.ToLower());
            content += " Добавлен комментарий: " + @event.Event?.Comment.Content + Environment.NewLine;
            content += FullName(@event.Event?.Author) + Environment.NewLine;
            content += $"{@event?.Issue?.Client?.Company?.Name}" + Environment.NewLine + Environment.NewLine;
            content += $"{endp.Value.OkdeskDomainUrl}/issues/{@event?.Issue?.Id}";

            await tgNotif.SendMessage(tgSettings.Value.ChatId, content, ct);
        }

        private static string Priority(string? priority) => priority switch
        {
            // Записывает приоритет заявки добавляя символ для читаемости в телеграмме
            "low" => "⚪️", // white circle
            "normal" => "🟢", // green circle
            "high" => "🔴", // red circle
            "block" => "🆘", // sos sqare
            "project" => "🟡", // yellow circle  
            _ => string.Empty
        };

        private static string FullName(EmployeeWebHook? author)
        {
            string fullname = string.Empty;
            if (author == null) return fullname;

            // Записывает ФИО
            if (!string.IsNullOrEmpty(author.Last_name))
                fullname += author.Last_name;
            if (!string.IsNullOrEmpty(author.First_name))
                fullname += " " + author.First_name;
            if (!string.IsNullOrEmpty(author.Patronymic))
                fullname += " " + author.Patronymic;

            return fullname;
        }
    }
}
