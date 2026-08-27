# Этап 8. Архитектурные тесты DI и границ production-слоёв

Status: Completed

## Цель

Закрепить автоматическими тестами структуру DI после разделения registrations между `CRMService.Application`, `CRMService.Infrastructure` и `CRMService.Web`, не изменяя бизнес-логику и production-поведение.

## Защищённые инварианты

- `ProjectReference` направлены от верхних слоёв к нижним; Web остаётся верхним composition root.
- Каждый production-слой предоставляет единственный публичный aggregate: `AddApplication`, `AddInfrastructure` или `AddWeb`.
- Старый публичный `ConfigureServices` и параллельные aggregate paths отсутствуют.
- Application, Infrastructure и Web регистрируют реализации только в разрешённых им границах.
- Domain и Contracts не содержат пустых DI extension-классов.
- `IStartupInitializer` и `MainDatabaseStartupInitializer` принадлежат Web; Infrastructure не зависит от Web.
- Критические DbContext, adapters, session, scenario Unit of Work, repositories и Okdesk sources имеют scoped lifetime и не дублируются.
- `EntitySyncService` и Web coordination services остаются singleton, `IStartupInitializer` — scoped, `ExceptionHandlingMiddleware` — transient.
- Typed HTTP clients и options configuration не дублируются.
- `IWebhookHandler` разрешаются в порядке Issue, Company, MaintenanceEntity, Equipment.
- Test host заменяет production DbContext, startup initializer, HTTP/notification services, authentication, webhook handlers и hosted services после production aggregates.
- Smart authentication направляет Bearer-запросы в JWT, остальные запросы — в Cookie; API возвращает 401/403 без redirect, Razor Pages сохраняют redirects.
- Environment `Testing` использует ephemeral Data Protection и не создаёт production key directories.

## Реализация

В `CRMService.Architecture.Tests` добавлены проверки project files, публичной DI surface и значимых `ServiceDescriptor`. Корень репозитория находится обходом родительских директорий без абсолютного пути. Общий production provider строится с in-memory configuration, environment `Testing`, `ValidateScopes` и `ValidateOnBuild` без открытия БД и исходящей сети.

В `CRMService.Web.IntegrationTests` усилена проверка controlled replacements в `CrmWebApplicationFactory`, порядка тестовых webhook handlers, test authentication scheme, Smart selector и удаления production hosted-service descriptors.

## Результаты проверки

- `CRMService.Architecture.Tests`: 12 passed, 0 failed, 0 skipped; build — 0 warnings.
- `CRMService.Web.IntegrationTests`: 46 passed, 0 failed, 0 skipped; build — 0 warnings.
- `ValidateScopes` и `ValidateOnBuild` прошли.
- Production-код, contracts, packages, executable hooks, schema и migrations не изменялись.
- Реальная сеть, БД, Docker, production jobs и production secrets не использовались.

## Связь с этапом 07

Этап 08 закрывает узкую DI-часть архитектурных проверок. CI jobs, публикация coverage/TRX и управление coverage baseline завершены отдельно в этапе 07.
