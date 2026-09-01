# Сервисы базы данных

Каталог содержит SQL Server backup и startup-проверку подключения/migrations для EF Core-контекстов.

## Правила сопровождения

- `SqlServerBackupService` работает только с SQL Server и получает настройки через `DatabaseBackupOptions`.
- `DatabaseBackup:WindowsSqlServerPath` и `DatabaseBackup:LinuxSqlServerPath` — абсолютные пути в файловой системе SQL Server для соответствующей ОС; `SqlServerBackupService` выбирает путь по ОС процесса ASP.NET Core, так как приложение и БД разворачиваются на одной ОС.
- Приложение не создаёт каталог backup; он должен существовать и быть доступен службе или контейнеру SQL Server.
- `DatabaseBackup:ProjectName` входит в имя `.bak` и может содержать только буквы, цифры, дефисы и нижние подчёркивания.
- В Docker каталог backup монтируется в контейнер SQL Server отдельным named volume; контейнер CRMService этот volume не монтирует.
- Перед применением pending migrations `DataBaseCheckUpService` обязан успешно создать backup; ошибки не маскировать.
- Все операции подключения, backup и migrations выполнять асинхронно с передачей `CancellationToken`.
