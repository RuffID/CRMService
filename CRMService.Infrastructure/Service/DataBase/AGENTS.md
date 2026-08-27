# Сервисы базы данных

Каталог содержит SQL Server backup и startup-проверку подключения/migrations для EF Core-контекстов.

## Правила сопровождения

- `SqlServerBackupService` работает только с SQL Server и получает настройки через `DatabaseBackupOptions`.
- `DatabaseBackup:SqlServerPath` — абсолютный путь в файловой системе SQL Server. Приложение не создаёт этот каталог и не обязано видеть его.
- `DatabaseBackup:ProjectName` входит в имя `.bak` и может содержать только буквы, цифры, дефисы и нижние подчёркивания.
- В Docker каталог backup монтируется в контейнер SQL Server отдельным named volume; контейнер CRMService этот volume не монтирует.
- Перед применением pending migrations `DataBaseCheckUpService` обязан успешно создать backup; ошибки не маскировать.
- Все операции подключения, backup и migrations выполнять асинхронно с передачей `CancellationToken`.
