# Этап 2. Декомпозиция Unit of Work

Status: Completed

## Проблема

Текущие `IUnitOfWork` и `UnitOfWork` агрегируют репозитории авторизации, CRM-настроек, отчётов и всех локальных сущностей Okdesk. Конструктор инфраструктурной реализации содержит десятки зависимостей, а application services и web controllers получают доступ к данным, которые не относятся к их сценарию. Это god object: он скрывает реальные зависимости, усложняет unit-тесты и повышает связанность модулей.

`IOkdeskUnitOfWork` меньше по поведению, но повторяет ту же проблему для PostgreSQL-источника: единый контракт предоставляет все облачные репозитории сразу. Его также нужно разделить, иначе после удаления основного god object останется второй.

## Фактические consumers до декомпозиции

| Consumer | MainContext repositories/operations | OkdeskContext repositories | Новая граница |
|---|---|---|---|
| `Authorization/UserService` | `User`, `CrmRole`, `Employee`, save | — | `IAuthorizationUnitOfWork` |
| `Authorization/RoleService` | `CrmRole` | — | `IAuthorizationUnitOfWork` |
| `Report/EmployeePerformanceReportService` | `EmployeePerformanceReport`, `Employee`, `EmployeeGroup`, `Plan`, `PlanSetting` | — | `IReportsUnitOfWork` |
| `Report/SpentTimeChartService` | `SpentTimeChartReport`, `Employee`, `EmployeeGroup`, `Group` | — | `IReportsUnitOfWork` |
| `Report/IssueDynamicsChartService` | `IssueDynamicsChartReport` | — | `IReportsUnitOfWork` |
| `CrmServices/PlanSettingsService` | `Plan`, `GeneralSettings`, `PlanSetting`, `PlanColor`, save | — | `IPlanSettingsUnitOfWork` |
| `CompanyService` | `Company`, `CompanyCategory`, save | `Company` | `ICompanyDirectoryUnitOfWork` + `IOkdeskCompanyDirectorySource` |
| `CompanyCategoryService` | `CompanyCategory`, save | `CompanyCategory` | `ICompanyDirectoryUnitOfWork` + `IOkdeskCompanyDirectorySource` |
| `EmployeeService` | `Employee`, save | `Employee` | `ICompanyDirectoryUnitOfWork` + `IOkdeskCompanyDirectorySource` |
| `GroupService` | `Group`, `EmployeeGroup`, save | `Group` | `ICompanyDirectoryUnitOfWork` + `IOkdeskCompanyDirectorySource` |
| `OkdeskEntity/RoleService` | `OkdeskRole`, `EmployeeRole`, save | — | `ICompanyDirectoryUnitOfWork` |
| `EquipmentService` | `Equipment`, `Parameter`, `Company`, `MaintenanceEntity`, `Manufacturer`, `Kind`, `Model`, `KindParameter`, save | `Equipment` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| `KindService` | `Kind`, save | `Kind` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| `KindParameterService` | `KindParameter`, save | `KindParameter` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| `KindParamService` | `Kind`, `KindParameter`, `KindParams`, save | `KindParams` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| `MaintenanceEntityService` | `MaintenanceEntity`, `Company`, save | `MaintenanceEntity` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| `ManufacturerService` | `Manufacturer`, save | `Manufacturer` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| `ModelService` | `Model`, save | `Model` | `IEquipmentUnitOfWork` + `IOkdeskEquipmentSource` |
| equipment resolvers (`KindParameter`, `Kind`, `MaintenanceEntity`, `Manufacturer`, `Model`) | одноимённые repositories | — | `IEquipmentUnitOfWork` |
| `IssueService` | `Issue`, `Employee`, `Company`, `MaintenanceEntity`, `IssueStatus`, `IssueType`, `IssuePriority`, save | `Issue`, `Employee` | `IIssuesUnitOfWork` + `IOkdeskIssuesSource` |
| `IssuePriorityService` | `IssuePriority`, save | `IssuePriority` | `IIssuesUnitOfWork` + `IOkdeskIssuesSource` |
| `IssueStatusService` | `IssueStatus`, save | `IssueStatus` | `IIssuesUnitOfWork` + `IOkdeskIssuesSource` |
| `IssueTypeService` | `IssueType`, `IssueTypeGroup`, save | `IssueType`, `IssueTypeGroup` | `IIssuesUnitOfWork` + `IOkdeskIssuesSource` |
| `TimeEntryService` | `TimeEntry`, `Issue`, `Employee`, save | `TimeEntry` | `IIssuesUnitOfWork` + `IOkdeskIssuesSource` |
| issue resolvers (`Company`, `Employee`, `IssuePriority`, `IssueStatus`, `IssueType`) | одноимённые repositories | — | `IIssuesUnitOfWork` |
| `LoginController` | `User`, `Session`, save | — | координация перенесена в `AuthenticationService` → `IAuthorizationUnitOfWork` |
| `Pages/Login` и `CookieAuthorizeAttribute` | `User` | — | `AuthenticationService` → `IAuthorizationUnitOfWork` |
| `CategoryController` | `CompanyCategory`, save | — | `CompanyCategoryService` → `ICompanyDirectoryUnitOfWork` |
| `CompanyController` | `Company` | — | `CompanyService` → `ICompanyDirectoryUnitOfWork` |
| `EmployeeController` | `Employee`, `EmployeeGroup` | — | `EmployeeService` → `ICompanyDirectoryUnitOfWork` |
| `EquipmentController` | `Equipment` | — | `EquipmentService` → `IEquipmentUnitOfWork` |
| `IssueController` | `Issue` | — | `IssueService` → `IIssuesUnitOfWork` |
| `IssuePriorityController`, `IssueStatusController`, `IssueTypeController` | соответствующий справочник | — | соответствующий Application service → `IIssuesUnitOfWork` |
| `KindController`, `KindParameterController`, `MaintenanceEntityController`, `ManufacturerController`, `ModelController` | соответствующий справочник | — | соответствующий Application service → `IEquipmentUnitOfWork` |
| Okdesk `RoleController` | `OkdeskRole` | — | `RoleService` → `ICompanyDirectoryUnitOfWork` |
| `DailyReportHostedService`, `ThirtyMinutesReportHostedService` | `Issue` | — | `IssueService` → `IIssuesUnitOfWork` |

Полностью закомментированные legacy authorization controllers не являлись runtime-consumers и удалены вместе с мёртвым кодом.

## Целевая модель

В Application создать общий технический контракт без репозиториев:

```csharp
public interface IUnitOfWorkScope
{
    Task SaveChangesAsync(CancellationToken ct = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken ct = default);
}
```

Возвращаемый тип `SaveChangesAsync` на первом этапе сохранить как `Task`, чтобы декомпозиция не меняла существующий контракт сохранения одновременно с границами ответственности. Переход на `Task<int>` возможен отдельной осознанной правкой.

Каждый сценарный Unit of Work наследует `IUnitOfWorkScope` и открывает только необходимые репозитории. Предварительный набор границ, который нужно подтвердить по фактическим consumers:

- `IAuthorizationUnitOfWork` — users, roles, user-role links, sessions и block reasons;
- `IPlanSettingsUnitOfWork` — plans, general settings, plan settings, plan colors и минимально необходимая выборка employees;
- `IReportsUnitOfWork` — три агрегирующих report repositories и только те plan/settings repositories, которые нужны расчётам;
- `ICompanyDirectoryUnitOfWork` — companies, categories, employees, groups и связи сотрудников;
- `IEquipmentUnitOfWork` — equipment, parameters, kinds, kind parameters, manufacturers, models, maintenance entities и необходимые company references;
- `IIssuesUnitOfWork` — issues, statuses, priorities, types, type groups, time entries и необходимые references;
- отдельный узкий Unit of Work добавлять только для сценария, который не укладывается в эти границы без предоставления лишних репозиториев.

Не создавать новый `ICrmUnitOfWork`, который снова объединит все перечисленные контракты.

## Облачный источник Okdesk

Текущий `IOkdeskUnitOfWork` разделить по предметным группам, например:

- `IOkdeskCompanyDirectoryUnitOfWork`;
- `IOkdeskEquipmentUnitOfWork`;
- `IOkdeskIssuesUnitOfWork`.

Если группа выполняет только чтение и не имеет общего сохранения/транзакции, не заставлять её наследовать `IUnitOfWorkScope` формально. Она может остаться узким агрегатором repository contracts либо быть заменена прямыми зависимостями от одного-двух repositories. Название `UnitOfWork` использовать только там, где действительно есть единая рабочая сессия или группа согласованных операций.

## Infrastructure

Создать общую scoped-реализацию технических операций MainContext, например `MainDbUnitOfWorkScope`. Сценарные реализации должны делегировать ей сохранение и транзакции и получать только собственные repositories. Все repositories и scopes внутри одного DI scope обязаны использовать один `IAppDbContext<MainContext>` и общий change tracker.

Для каждого сценарного контракта создать отдельную реализацию:

```text
AuthorizationUnitOfWork
PlanSettingsUnitOfWork
ReportsUnitOfWork
CompanyDirectoryUnitOfWork
EquipmentUnitOfWork
IssuesUnitOfWork
```

Точные названия и состав определяются по реальным методам consumers, а не по структуре папок. EF Core types, `IQueryable`, tracking flags и `TContext` не должны попасть в Application contracts.

## Порядок миграции

1. Составить таблицу всех consumers `IUnitOfWork`/`IOkdeskUnitOfWork` и используемых ими свойств.
2. Сгруппировать consumers по бизнес-сценариям и подтвердить границы выше.
3. Добавить `IUnitOfWorkScope` и его инфраструктурную реализацию с characterization tests для save, commit и rollback.
4. Добавлять по одному сценарному Unit of Work вместе с регистрацией DI и unit/integration tests.
5. Переводить application services на минимальный контракт. Если controller напрямую работает с repository/UoW, сначала перенести сценарий в Application service, сохранив controller тонкой HTTP-обёрткой.
6. Перевести Okdesk services на разделённые source Unit of Work или прямые repository dependencies.
7. После каждого переноса выполнить поиск оставшихся consumers старых контрактов.
8. Удалить `IUnitOfWork`, `UnitOfWork`, `IOkdeskUnitOfWork` и `OkdeskUnitOfWork` только когда ссылок на них не осталось.
9. Обновить DI registrations и ближайшие локальные `AGENTS.md`, описав новые границы.

## Тестовое сопровождение

- Application tests подменяют один небольшой сценарный Unit of Work, а не десятки несвязанных repositories.
- Изолированные Infrastructure tests подтверждают делегирование save/commit/rollback/cancellation общей MainContext-сессии без подключения к БД; provider-level container coverage выполняется на этапе 05.
- Для общей сессии сценарных Unit of Work проверить save, commit, rollback и cancellation.
- Architecture tests запрещают зависимость Application от старых глобальных UoW и не позволяют сценарному контракту разрастись до всех repositories.
- DI smoke test разрешает все сценарные Unit of Work с `ValidateScopes` и `ValidateOnBuild`.

## Критерии завершения

- Старые глобальные Unit of Work удалены.
- Ни один application service или controller не получает доступ к несвязанным repositories через агрегирующий контракт.
- Сохранение и транзакции остаются едиными для repositories одного сценария.
- Application contracts не содержат EF Core деталей.
- Все новые границы покрыты unit и architecture tests; общая save/transaction-сессия покрыта изолированными Infrastructure tests. Container integration относится к этапу 05.

## Текущее состояние

Глобальные `IUnitOfWork`/`UnitOfWork` и `IOkdeskUnitOfWork`/`OkdeskUnitOfWork` удалены. Consumers и DI переведены на шесть MainContext-границ и три read-only Okdesk source-контракта. Все Application repository-контракты переведены с EFCoreLibrary-интерфейсов на предметные методы: в публичных сигнатурах больше нет `DbContext`, `IQueryable`, expressions, tracking/include-параметров или EF Core types. Query composition, includes, tracking и Okdesk discriminator queries находятся в Infrastructure.

Добавлены unit/static tests границ и изолированные tests общей save/transaction-сессии без БД, Docker, host и внешней сети. Provider-level container tests не добавлялись: они относятся к этапу 05 и исключены из этапа 02.
