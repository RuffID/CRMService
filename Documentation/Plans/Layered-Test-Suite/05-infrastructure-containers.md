# Этап 5. Infrastructure и самоликвидирующиеся Docker-контейнеры

Status: Not Started

## Пакеты и разделение suite

В `CRMService.Infrastructure.IntegrationTests` добавить Testcontainers modules для SQL Server и PostgreSQL, а также `Respawn` для очистки данных. Чистые инфраструктурные компоненты (`JwtTokenService`, HTTP adapters) тестировать в том же проекте без подключения container fixture.

Контейнерные тесты пометить `[Trait("Dependency", "Docker")]` и сгруппировать в отдельные xUnit v3 collections:

- `SqlServerCollection` для `MainContext`;
- `PostgreSqlCollection` для `OkdeskContext`.

Внутри каждой collection тесты выполняются последовательно относительно общей БД; SQL Server и PostgreSQL collections могут выполняться параллельно, если CI имеет достаточно ресурсов.

## Жизненный цикл контейнеров

Создать по одной assembly/collection fixture на provider с `IAsyncLifetime`:

1. Сформировать уникальное имя контейнера и database.
2. Запустить container на динамическом host port без bind mounts.
3. Дождаться readiness через штатный wait strategy Testcontainers.
4. Инициализировать схему.
5. Перед каждым тестом очищать данные через Respawn либо создавать уникальную database для теста, если проверяются migrations.
6. В `DisposeAsync` закрыть DbContext/connection и вызвать `StopAsync`/`DisposeAsync` контейнера в `finally`.

Оставить Testcontainers Resource Reaper (Ryuk) включённым и не включать reuse. Это даёт два уровня уборки: явный dispose при нормальном завершении и удаление помеченных ресурсов при аварийном завершении процесса. Образы закрепить конкретными проверенными tags, не использовать `latest`. Для SQL Server явно принять лицензию в fixture. Секреты и production connection strings не читать.

## Инициализация схемы

- `MainContext`: на чистой SQL Server database выполнять `MigrateAsync`, тем самым проверяя production migrations. Отдельный smoke test должен подтвердить применение всех migrations с нуля.
- `OkdeskContext`: migrations сейчас отсутствуют, поэтому для тестовой database применять `EnsureCreatedAsync`. Если появятся production migrations для этого контекста, заменить этот шаг на `MigrateAsync`.
- Не использовать production-сервис автоматического backup/migration как fixture initializer: он смешивает проверку подключения, backup и migration.

## Набор инфраструктурных тестов

### EF Core и repositories

- конфигурации таблиц, ключей, required/nullable, max length, relations, delete behavior и converters;
- CRUD и специализированные repository queries;
- tracking/no-tracking там, где это влияет на обновление;
- фильтрация, сортировка и pagination заявок/оборудования;
- report aggregate repositories на данных с граничными датами и пустыми выборками;
- `UnitOfWork.SaveChangesAsync` и commit/rollback `ExecuteInTransaction`;
- `OkdeskUnitOfWork` и cloud repositories на PostgreSQL.

### Инфраструктурные сервисы без внешней сети

- `JwtTokenService`: claims, issuer, audience, expiration и ошибка отсутствующего ключа;
- `GetOkdeskEntityService`: pagination, limit, 404 mapping, cancellation и прочие HTTP-ошибки через fake `IHttpApiClient`;
- `TelegramNotification`: method, URL, header, JSON body, invalid input, unsuccessful response и cancellation через fake `HttpMessageHandler`;
- `BackupService`: отдельный SQL Server container scenario с container-local backup path; не писать backup на host и очищать файл вместе с контейнером;
- `DataBaseCheckUpService`: connection failure, отсутствие migrations и pending migrations проверять отдельно, не запускать его автоматически для всех тестов.

## Fail-fast и эксплуатация

- Обычный pipeline исключает Docker cases через Microsoft.Testing.Platform `--filter-not-trait "Dependency=Docker"`.
- Container pipeline сначала явно проверяет Docker, затем запускает `--filter-trait "Dependency=Docker"`; недоступность Docker завершает job ошибкой.
- Логи контейнеров прикладывать только при падении.
- Ограничить timeout старта и выполнения, чтобы зависший Docker не блокировал CI бессрочно.

## Критерии завершения

- После успешного и упавшего тестового прогона не остаётся контейнеров, volumes и host-файлов.
- Оба provider проверяются реальными движками.
- Тесты не зависят от порядка и production data.
