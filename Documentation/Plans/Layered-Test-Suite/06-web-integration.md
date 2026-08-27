# Этап 6. Web integration tests

## Подготовительный рефакторинг для тестируемости

До создания `WebApplicationFactory` нужны небольшие явные seams:

1. Добавить доступный тестам `public partial class Program` без переноса прикладных регистраций обратно в `Program.cs`.
2. Сделать startup-проверку `DataBaseCheckUpService<MainContext>` заменяемой/отключаемой тестовой регистрацией. Production-поведение остаётся fail-fast; test host не должен автоматически выполнять backup или migration.
3. Разделить конфигурацию DI так, чтобы test factory могла заменить оба DbContext и внешние HTTP clients после production registrations.
4. Устранить запись Data Protection keys в рабочий каталог test host: использовать test-only ephemeral provider либо уникальный временный каталог с обязательным удалением.
5. Передавать тестовую конфигурацию in-memory; не использовать реальные токены, connection strings и `Config/config.json` как источник секретов.

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
