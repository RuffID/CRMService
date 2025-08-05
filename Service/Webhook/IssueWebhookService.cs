using CRMService.API;
using CRMService.Interfaces.Api;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Models.WebHook;
using CRMService.Service.Entity;
using Microsoft.Extensions.Options;

namespace CRMService.Service.Webhook
{
    public class IssueWebhookService(IUnitOfWorkEntities unitOfWork, IssueService issueService, TimeEntryService timeEntryService, IOptions<ApiEndpoint> endp,
        IRequestService request, IOptions<TelegramBotSettings> tgSettings, ILoggerFactory logger) : IWebhookHandler
    {
        private const string AUTHOR_CONTACT_TYPE = "contact";
        private readonly TelegramNotification telegramNotification = new(request, endp.Value, logger);
        private readonly ILogger<IssueWebhookService> _logger = logger.CreateLogger<IssueWebhookService>();

        public async Task<bool> HandleWebhook(RootEvent @event)
        {
            if (@event.Issue == null)
                return false;

            switch (@event.Event!.Event_type)
            {
                case "new_ticket_status":
                    await UpdateStatusAndSaveTimeEntries(@event);
                    break;
                case "new_ticket":
                    await CreateIssue(@event.Issue);
                    break;
                case "new_assignee":
                    await UpdateIssue(@event.Issue);
                    break;
                case "update_issue_work_type":
                    await UpdateIssue(@event.Issue);
                    break;
                case "ticket_deleted":
                    await MarkIssueAsDeletedAsync(@event.Issue);
                    break;
                case "new_comment":
                    await NewComment(@event);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private async Task CreateIssue(IssueJSON issueJson)
        {
            Issue issue = issueJson.ConvertToIssue();

            await issueService.CheckAttributes(issue);
            await unitOfWork.Issue.CreateOrUpdate(issue);

            _logger.LogInformation("[Method:{MethodName}] Create issue from webhook: Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(CreateIssue), issueJson.Id, issueJson.Status?.Code, issueJson.Priority?.Code, issueJson.Type?.Code, issueJson.Client?.Company?.Id, issueJson.Maintenance_entity?.Id, issueJson.Assignee?.Employee?.Id);

            await unitOfWork.SaveAsync();

            string content = string.Empty;

            content += Priority(issueJson.Priority?.Code?.ToLower());
            content += $" {issue.Title}" + Environment.NewLine;
            content += $"{issueJson.Client?.Company?.Name}" + Environment.NewLine + Environment.NewLine;
            content += $"{endp.Value.OkdeskDomain}/issues/{issue.Id}";

            if (issueJson.Author?.Type == AUTHOR_CONTACT_TYPE)
                await telegramNotification.SendMessage(tgSettings.Value.SupportChatId, content);
        }

        private async Task UpdateStatusAndSaveTimeEntries(RootEvent @event)
        {
            Issue issue = @event.Issue!.ConvertToIssue();
            await issueService.CheckAttributes(issue);

            _logger.LogInformation("[Method:{MethodName}] Update issue from webhook: \"{WebhookType}\". Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(UpdateStatusAndSaveTimeEntries), @event.Event!.Event_type, @event.Issue.Id, @event.Issue.Status?.Code, @event.Issue.Priority?.Code, @event.Issue.Type?.Code, @event.Issue.Client?.Company?.Id, @event.Issue.Maintenance_entity?.Id, @event.Issue.Assignee?.Employee?.Id);

            await unitOfWork.Issue.CreateOrUpdate(issue);
            await unitOfWork.SaveAsync();

            List<TimeEntry> entries = [];
            if (@event.Event!.Time_entries != null && @event.Event!.Time_entries.Length != 0)
            {
                foreach (TimeEntryWebHook entry in @event.Event.Time_entries)
                {
                    entries.Add(new()
                    {
                        Id = entry.Id,
                        SpentTime = entry.Spent_time,
                        EmployeeId = entry.Employee?.Id ?? 0,
                        IssueId = issue.Id,
                        CreatedAt = DateTime.Now,
                        LoggedAt = entry.Logged_at
                    });
                }

                foreach (TimeEntry? entry in entries)
                    await timeEntryService.CheckEmployeeAndIssue(entry);

                await unitOfWork.TimeEntry.CreateOrUpdate(entries);

                _logger.LogInformation("[Method:{MethodName}] Create time entry from webhook: \"{WebhookType}\". Time entries count: {timeEntriesCount}, issueId: {issueId}, assigneeId: {assigneeId}",
                nameof(UpdateStatusAndSaveTimeEntries), @event.Event!.Event_type, entries.Count, @event.Issue.Id, @event.Issue.Assignee?.Employee?.Id);

                await unitOfWork.SaveAsync();
            }
        }

        private async Task UpdateIssue(IssueJSON issueJson)
        {
            Issue issue = issueJson.ConvertToIssue();
            await issueService.CheckAttributes(issue);
            await unitOfWork.Issue.CreateOrUpdate(issue);

            _logger.LogInformation("[Method:{MethodName}] Update issue from webhook: Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(UpdateIssue), issueJson.Id, issueJson.Status?.Code, issueJson.Priority?.Code, issueJson.Type?.Code, issueJson.Client?.Company?.Id, issueJson.Maintenance_entity?.Id, issueJson.Assignee?.Employee?.Id);

            await unitOfWork.SaveAsync();
        }

        private async Task MarkIssueAsDeletedAsync(IssueJSON issueJson)
        {
            Issue convertIssue = issueJson.ConvertToIssue();
            await issueService.CheckAttributes(convertIssue);

            convertIssue.DeletedAt = DateTime.Now;
            await unitOfWork.Issue.CreateOrUpdate(convertIssue);

            _logger.LogInformation("[Method:{MethodName}] Delete issue from webhook: Issue: {issueId}, status: {statusCode}, priority: {priorityCode}, old work type: {typeCode}, companyId: {companyId}, objectId: {objectId}, assigneeId: {assigneeId}",
                nameof(MarkIssueAsDeletedAsync), issueJson.Id, issueJson.Status?.Code, issueJson.Priority?.Code, issueJson.Type?.Code, issueJson.Client?.Company?.Id, issueJson.Maintenance_entity?.Id, issueJson.Assignee?.Employee?.Id);

            await unitOfWork.SaveAsync();
        }

        private async Task NewComment(RootEvent @event)
        {
            if (@event.Event?.Author?.Type != "contact" || string.IsNullOrEmpty(@event.Event?.Comment?.Content))
                return;

            DateTime evening = DateTime.Now;
            evening = new DateTime(evening.Year, evening.Month, evening.Day, hour: 18, minute: 0, second: 0);
            DateTime morning = new(evening.Year, evening.Month, evening.Day, hour: 9, minute: 0, second: 0);

            // Если новый комментарий от клиента был добавлен после 09:00 утра и раньше 18:00 вечера, то уведомлять не нужно

            if (DateTime.Now > morning && DateTime.Now < evening)
                return;

            // Не уведомлять при объединении заявок
            if (@event.Event?.Comment?.Content.Contains("Комментарий добавлен при объединении заявок") == true)
                return;

            string content = string.Empty;
            content += Priority(@event?.Issue?.Priority?.Code?.ToLower());
            content += " Добавлен комментарий: " + @event?.Event?.Comment?.Content + Environment.NewLine;
            content += FullName(@event?.Event?.Author) + Environment.NewLine;
            content += $"{@event?.Issue?.Client?.Company?.Name}" + Environment.NewLine + Environment.NewLine;
            content += $"{endp.Value.OkdeskDomain}/issues/{@event?.Issue?.Id}";

            await telegramNotification.SendMessage(tgSettings.Value.SupportChatId, content);
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
