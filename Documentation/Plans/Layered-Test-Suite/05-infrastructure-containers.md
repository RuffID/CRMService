# Этап 5. Infrastructure и самоликвидирующиеся Docker-контейнеры

Status: Completed

## Обнаруженные и исправленные неоднозначности

- Generic `BackupService<TContext>` создавал `backupFolder` в файловой системе приложения, хотя `BACKUP DATABASE` записывает файл в файловой системе SQL Server. Сервис заменён на `SqlServerBackupService` с typed `DatabaseBackupOptions`: путь валидируется как абсолютный SQL Server-visible path и не создаётся приложением. Для Docker задокументирован отдельный named volume, монтируемый только в SQL Server.
- Production migration `20260307072417_UpdatePriorityAndStatusModels` не применялась с нуля на SQL Server 2022: `DBCC CHECKIDENT` получал переменную как третий параметр. Лишние reseed-блоки удалены; перенос идентификаторов остаётся под `IDENTITY_INSERT`, а container regression test подтверждает последующую генерацию identity.

## Текущее состояние

Добавлена SQL Server collection с Testcontainers `4.14.0`, Respawn `6.2.1`, pinned image, динамическим port и уникальными container/database names. SQL Server применяет все production migrations через `MigrateAsync`. PostgreSQL collection и связанные с ней тесты удалены. После прогонов container, volumes и host backup files не остаются.

## Пакеты и разделение suite

В `CRMService.Infrastructure.IntegrationTests` использовать Testcontainers module для SQL Server и `Respawn` для очистки данных. Чистые инфраструктурные компоненты (`JwtTokenService`, HTTP adapters) тестировать в том же проекте без подключения container fixture.

Контейнерные тесты пометить `[Trait("Dependency", "Docker")]` и сгруппировать в xUnit v3 collection:

- `SqlServerCollection` для `MainContext`.

Внутри collection тесты выполняются последовательно относительно общей БД.

## Жизненный цикл контейнеров

Создать collection fixture с `IAsyncLifetime`:

1. Сформировать уникальное имя контейнера и database.
2. Запустить container на динамическом host port без bind mounts.
3. Дождаться readiness через штатный wait strategy Testcontainers.
4. Инициализировать схему.
5. Перед каждым тестом очищать данные через Respawn либо создавать уникальную database для теста, если проверяются migrations.
6. В `DisposeAsync` закрыть DbContext/connection и вызвать `StopAsync`/`DisposeAsync` контейнера в `finally`.

Оставить Testcontainers Resource Reaper (Ryuk) включённым и не включать reuse. Это даёт два уровня уборки: явный dispose при нормальном завершении и удаление помеченных ресурсов при аварийном завершении процесса. Образы закрепить конкретными проверенными tags, не использовать `latest`. Для SQL Server явно принять лицензию в fixture. Секреты и production connection strings не читать.

## Инициализация схемы

- `MainContext`: на чистой SQL Server database выполнять `MigrateAsync`, тем самым проверяя production migrations. Отдельный smoke test должен подтвердить применение всех migrations с нуля.
- Не использовать production-сервис автоматического backup/migration как fixture initializer: он смешивает проверку подключения, backup и migration.

## Набор инфраструктурных тестов

### EF Core и repositories

- конфигурации таблиц, ключей, required/nullable, max length, relations, delete behavior и converters;
- CRUD и специализированные repository queries;
- tracking/no-tracking там, где это влияет на обновление;
- фильтрация, сортировка и pagination заявок/оборудования;
- report aggregate repositories на данных с граничными датами и пустыми выборками;
- `UnitOfWork.SaveChangesAsync` и commit/rollback `ExecuteInTransaction`;

### Инфраструктурные сервисы без внешней сети

- `JwtTokenService`: claims, issuer, audience, expiration и ошибка отсутствующего ключа;
- `GetOkdeskEntityService`: pagination, limit, 404 mapping, cancellation и прочие HTTP-ошибки через fake `IHttpApiClient`;
- `TelegramNotification`: method, URL, header, JSON body, invalid input, unsuccessful response и cancellation через fake `HttpMessageHandler`;
- `SqlServerBackupService`: отдельный SQL Server container scenario с container-local backup path; не писать backup на host и очищать файл вместе с контейнером;
- `DataBaseCheckUpService`: connection failure, отсутствие migrations и pending migrations проверять отдельно, не запускать его автоматически для всех тестов.

## Fail-fast и эксплуатация

- Обычный pipeline исключает Docker cases через Microsoft.Testing.Platform `--filter-not-trait "Dependency=Docker"`.
- Container pipeline сначала явно проверяет Docker, затем запускает `--filter-trait "Dependency=Docker"`; недоступность Docker завершает job ошибкой.
- Логи контейнеров прикладывать только при падении.
- Ограничить timeout старта и выполнения, чтобы зависший Docker не блокировал CI бессрочно.

## Критерии завершения

- После успешного и упавшего тестового прогона не остаётся контейнеров, volumes и host-файлов.
- SQL Server проверяется реальным движком.
- Тесты не зависят от порядка и production data.
