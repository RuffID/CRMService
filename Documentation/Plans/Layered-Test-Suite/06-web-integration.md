# Этап 6. Web integration tests

Status: Completed

## Обнаруженные дефекты и неоднозначности

- Generic `JsonResultMapper` возвращал успешные данные напрямую вместо документированного envelope `{ success: true, data: ... }`. Этап 06 исправляет контракт точечно и закрепляет regression tests.
- `ExceptionHandlingMiddleware` не различает application и infrastructure exceptions: все неизвестные исключения имеют единый безопасный ответ `500`, cancellation — `499`. Новую классификацию без существующего production-контракта этап не вводит.
- В окружениях вне Development встроенный `UseExceptionHandler` был расположен после custom middleware и перехватывал исключения раньше него. Из-за этого API получал HTML `500`, а контракт `ExceptionHandlingMiddleware`, включая `499` для cancellation, был недостижим. Порядок middleware исправлен regression HTTP tests.
- Webhook после `204 No Content` намеренно обрабатывается независимо от request lifetime с `CancellationToken.None`; request cancellation после принятия события к handler не передаётся. Изменение этого fire-and-forget контракта требует отдельного продуктового решения.

## Подготовительный рефакторинг для тестируемости

До создания `WebApplicationFactory` нужны небольшие явные seams:

1. Сделать startup-проверку `DataBaseCheckUpService<MainContext>` заменяемой/отключаемой тестовой регистрацией. Production-поведение остаётся fail-fast; test host не должен автоматически выполнять backup или migration.
2. Разделить конфигурацию DI так, чтобы test factory могла заменить оба DbContext и внешние HTTP clients после production registrations.
3. Устранить запись Data Protection keys в рабочий каталог test host: использовать test-only ephemeral provider либо уникальный временный каталог с обязательным удалением.
4. Передавать тестовую конфигурацию in-memory; не использовать реальные токены, connection strings и `Config/config.json` как источник секретов.

Не добавлять `public partial class Program` только ради тестов: test factory должна использовать существующую точку входа/доступный тип Web assembly без изменения `Program.cs`, если фактическая конфигурация `WebApplicationFactory` работает без такого seam.

Эти изменения выполнять отдельными точечными commits/этапами и сначала зафиксировать существующее поведение smoke-тестами.

## WebApplicationFactory

Создать `CrmWebApplicationFactory`:

- environment `Testing`;
- валидные in-memory Options и signing key;
- удалённые/заменённые hosted services;
- fake внешние HTTP clients;
- управляемая authentication scheme для тестов прав;
- при endpoint-тестах с БД — connection strings контейнерных fixtures;
- явное освобождение `HttpClient`, host, scopes и временного каталога.

Контроллеры и Razor handlers без необходимости в полной БД можно тестировать через подменённые application services. Сквозные database endpoints помечать `Dependency=Docker` и переиспользовать fixtures этапа Infrastructure без создания второго несовместимого механизма контейнеров.

## Приоритетные проверки

- `JsonResultMapper`: success/failure, status code и JSON payload;
- middleware исключений: ожидаемые application/infrastructure exceptions и неизвестная ошибка;
- cookie/JWT authentication: 401 для API, redirect для Razor Pages, 403 при недостаточных ролях;
- `IpOkdeskWebHookActionFilter`: разрешённый/запрещённый адрес и корректная Options-конфигурация;
- webhook endpoint: выбор handlers, пустой/невалидный payload, cancellation;
- controllers: status codes, отсутствие бизнес-логики в HTTP-обёртке и запуск background operation;
- Razor Page handlers для Users, Settings, Report, PlanSettings, Issues и Equipments;
- `ReportBackgroundService`: только с уникальным temporary content root и обязательной очисткой;
- DI smoke test: построение provider с `ValidateScopes` и `ValidateOnBuild`, разрешение ключевых services.

Не запускать production hosted jobs, backup, внешние API или реальную Telegram отправку.

## Критерии завершения

- Test host стартует без production secrets и side effects.
- Основные auth и error-handling контракты проверяются HTTP-запросами.
- Сквозные тесты с БД используют те же реальные provider и правила очистки, что Infrastructure suite.

## Текущее состояние

Добавлен заменяемый `IStartupInitializer`: production-реализация делегирует `DataBaseCheckUpService<MainContext>`, поэтому connection check, backup-before-migrate, migrations и fail-fast сохранены. В environment `Testing` обязательный production `Config/config.json` не загружается, Data Protection использует ephemeral provider, а `CrmWebApplicationFactory` передаёт только in-memory settings и тестовый signing key.

Factory использует существующий публичный `ReportBackgroundService` как marker Web assembly; изменение `Program` на `public partial class` не потребовалось. После production registrations factory заменяет оба DbContext тестовыми registrations без подключения, startup initializer, authentication, webhook handlers, `IHttpApiClient` и notification service; release hosted jobs удаляются по точным implementation types. Provider строится с `ValidateScopes` и `ValidateOnBuild`.

Добавлено 46 быстрых тестов JsonResult mapping, exception middleware, cookie/JWT и role authorization, IP/forwarded webhook filter, webhook dispatch/invalid payload/unknown event, controller/background operation contracts, Razor routes и handlers Users/Settings/Report/PlanSettings/Issues/Equipments, файлового report background service и DI smoke. Исправлены generic JSON envelope и порядок exception middleware. Быстрый suite: 46 passed, 0 failed, 0 skipped. Docker Web tests не создавались, потому что выбранные HTTP-контракты не зависят от реального SQL Server/PostgreSQL.
