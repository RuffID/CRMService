# Этап 2. Декомпозиция Unit of Work

## Проблема

Текущие `IUnitOfWork` и `UnitOfWork` агрегируют репозитории авторизации, CRM-настроек, отчётов и всех локальных сущностей Okdesk. Конструктор инфраструктурной реализации содержит десятки зависимостей, а application services и web controllers получают доступ к данным, которые не относятся к их сценарию. Это god object: он скрывает реальные зависимости, усложняет unit-тесты и повышает связанность модулей.

`IOkdeskUnitOfWork` меньше по поведению, но повторяет ту же проблему для PostgreSQL-источника: единый контракт предоставляет все облачные репозитории сразу. Его также нужно разделить, иначе после удаления основного god object останется второй.

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
- Infrastructure container tests подтверждают, что repositories одного сценария используют общий MainContext и transaction.
- Для каждого сценарного Unit of Work проверить save, commit, rollback и cancellation.
- Architecture tests запрещают зависимость Application от старых глобальных UoW и не позволяют сценарному контракту разрастись до всех repositories.
- DI smoke test разрешает все сценарные Unit of Work с `ValidateScopes` и `ValidateOnBuild`.

## Критерии завершения

- Старые глобальные Unit of Work удалены.
- Ни один application service или controller не получает доступ к несвязанным repositories через агрегирующий контракт.
- Сохранение и транзакции остаются едиными для repositories одного сценария.
- Application contracts не содержат EF Core деталей.
- Все новые границы покрыты unit, container integration и architecture tests.
