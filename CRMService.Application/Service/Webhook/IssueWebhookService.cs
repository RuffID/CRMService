using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Application.Models.WebHook;
using CRMService.Application.Service.OkdeskEntity;
using Microsoft.Extensions.Options;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.Webhook
{
    public class IssueWebhookService(
        IssueService issueService,
        TimeEntryService timeEntryService,
        IOptions<ApiEndpointOptions> endp,
        IOptions<TelegramBotOptions> tgSettings,
        EntitySyncService sync,
        INotificationService tgNotif,
        ILogger<IssueWebhookService> logger) : IWebhookHandler
    {
        private const string AUTHOR_CONTACT_TYPE = "contact";

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
                    await CreateIssue(@event.Issue, @event.Event!.Event_type, ct);
                    break;
                case "new_assignee":
                    await UpdateIssue(@event.Issue, @event.Event!.Event_type, ct);
                    break;
                case "update_issue_work_type":
                    await UpdateIssue(@event.Issue, @event.Event!.Event_type, ct);
                    break;
                case "new_priority":
                    await UpdateIssue(@event.Issue, @event.Event!.Event_type, ct);
                    break;
                case "ticket_deleted":
                    await MarkIssueAsDeletedAsync(@event.Issue, @event.Event!.Event_type, ct);
                    break;
                case "new_comment":
                    await NewComment(@event, ct);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private async Task CreateIssue(IssueWebHook issueJson, string eventType, CancellationToken ct)
        {
            Issue issue = issueJson.ConvertToIssue();

            await sync.RunExclusive(issue, async () =>
            {
                logger.LogInformation("[Method:{MethodName}] Create issue from webhook: \"{EventType}\" Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                    nameof(CreateIssue),
                    eventType,
                    issueJson.Id,
                    issueJson.Status?.Code,
                    issueJson.Priority?.Code,
                    issueJson.Type?.Code,
                    issueJson.Client?.Company?.Id,
                    issueJson.Maintenance_entity?.Id,
                    issueJson.EffectiveAssignee?.Employee?.Id);

                await issueService.CreateOrUpdateAsync(issue, ct);
            }, ct);

            string content = string.Empty;

            content += Priority(issueJson.Priority?.Code?.ToLower());
            content += $" {issue.Title}" + Environment.NewLine;
            content += $"{issueJson.Client?.Company?.Name}" + Environment.NewLine + Environment.NewLine;
            content += $"{endp.Value.OkdeskDomainUrl}/issues/{issue.Id}";

            if (issueJson.Author?.Type == AUTHOR_CONTACT_TYPE)
                await tgNotif.SendMessage(tgSettings.Value.SupportChatId, content, ct);
        }

        private async Task UpdateStatusAndSaveTimeEntries(RootEventWebHook @event, CancellationToken ct)
        {
            Issue issue = @event.Issue!.ConvertToIssue();

            await sync.RunExclusive(issue, async () =>
            {
                logger.LogInformation("[Method:{MethodName}] Update issue from webhook: \"{WebhookType}\". Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                    nameof(UpdateStatusAndSaveTimeEntries),
                    @event.Event!.Event_type,
                    @event.Issue.Id,
                    @event.Issue.Status?.Code,
                    @event.Issue.Priority?.Code,
                    @event.Issue.Type?.Code,
                    @event.Issue.Client?.Company?.Id,
                    @event.Issue.Maintenance_entity?.Id,
                    @event.Issue.EffectiveAssignee?.Employee?.Id);

                await issueService.CreateOrUpdateAsync(issue, ct);

                if (@event.Event.Time_entries == null || @event.Event.Time_entries.Length == 0)
                    return;

                List<TimeEntry> entries = new();

                logger.LogInformation("[Method:{MethodName}] Create time entry from webhook: \"{WebhookType}\". Time entries count: {timeEntriesCount}, issueId: {issueId}, assigneeId: {assigneeId}",
                nameof(UpdateStatusAndSaveTimeEntries),
                @event.Event.Event_type,
                @event.Event.Time_entries.Length,
                @event.Issue.Id,
                @event.Issue.EffectiveAssignee?.Employee?.Id);

                foreach (TimeEntryWebHook entry in @event.Event.Time_entries)
                {
                    TimeEntry newEntry = new()
                    {
                        Id = entry.Id,
                        SpentTime = entry.Spent_time,
                        EmployeeId = entry.Employee.Id,
                        IssueId = issue.Id,
                        LoggedAt = entry.Logged_at
                    };

                    await timeEntryService.CreateOrUpdate(newEntry, ct);
                }
            }, ct);
        }

        private async Task UpdateIssue(IssueWebHook issueJson, string eventType, CancellationToken ct)
        {
            Issue issue = issueJson.ConvertToIssue();

            await sync.RunExclusive(issue, async () =>
            {
                logger.LogInformation("[Method:{MethodName}] Update issue from webhook: \"{EventType}\". Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                    nameof(UpdateIssue),
                    eventType,
                    issueJson.Id,
                    issueJson.Status?.Code,
                    issueJson.Priority?.Code,
                    issueJson.Type?.Code,
                    issueJson.Client?.Company?.Id,
                    issueJson.Maintenance_entity?.Id,
                    issueJson.EffectiveAssignee?.Employee?.Id);

                await issueService.CreateOrUpdateAsync(issue, ct);
            }, ct);
        }

        private async Task MarkIssueAsDeletedAsync(IssueWebHook issueJson, string eventType, CancellationToken ct)
        {
            Issue issue = issueJson.ConvertToIssue();

            await sync.RunExclusive(issue, async () =>
            {
                logger.LogInformation("[Method:{MethodName}] Delete issue from webhook: \"{EventType}\". Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, old work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                    nameof(MarkIssueAsDeletedAsync),
                    eventType,
                    issueJson.Id,
                    issueJson.Status?.Code,
                    issueJson.Priority?.Code,
                    issueJson.Type?.Code,
                    issueJson.Client?.Company?.Id,
                    issueJson.Maintenance_entity?.Id,
                    issueJson.EffectiveAssignee?.Employee?.Id);

                issue.DeletedAt = DateTime.Now;

                await issueService.CreateOrUpdateAsync(issue, ct);
            }, ct);
        }

        private async Task NewComment(RootEventWebHook @event, CancellationToken ct)
        {
            if (@event.Event?.Author?.Type != AUTHOR_CONTACT_TYPE || string.IsNullOrEmpty(@event.Event?.Comment?.Content))
                return;

            logger.LogInformation("[Method:{MethodName}] New comment from webhook: \"{WebhookType}\". Issue: {issueId}, author: {authorFullName}, companyId: {companyId}",
                nameof(NewComment),
                @event.Event!.Event_type,
                @event.Issue!.Id,
                FullName(@event.Event.Author),
                @event.Issue.Client?.Company?.Id);

            DateTime now = DateTime.Now;
            DateTime evening = new(now.Year, now.Month, now.Day, hour: 18, minute: 0, second: 0);
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
            content += $"{@event.Issue?.Client?.Company?.Name}" + Environment.NewLine + Environment.NewLine;
            content += $"{endp.Value.OkdeskDomainUrl}/issues/{@event.Issue?.Id}";

            await tgNotif.SendMessage(tgSettings.Value.SupportChatId, content, ct);
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