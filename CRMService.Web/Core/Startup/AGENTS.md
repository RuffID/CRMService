# Startup initialization

Директория содержит узкий контракт startup-инициализации Web host и production-адаптер проверки `MainContext`.

Правила:
- production initializer делегирует `DataBaseCheckUpService<MainContext>` и сохраняет fail-fast, backup-before-migrate и cancellation;
- `Program.cs` разрешает только `IStartupInitializer`, не вызывает database service напрямую;
- test host заменяет контракт после production registrations и не запускает backup или migrations;
- не добавлять в общий initializer фоновые jobs или внешние интеграции.
