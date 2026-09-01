# CRMService.Infrastructure.IntegrationTests

Проект проверяет Infrastructure; разрешена ссылка только на `CRMService.Infrastructure`. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Обычные тесты не используют реальную сеть, БД и Docker. Контейнерные тесты маркируются `[Trait("Dependency", "Docker")]`: быстрые тесты выбираются через `--filter-not-trait "Dependency=Docker"`, Docker-тесты — через `--filter-trait "Dependency=Docker"`.

SQL Server использует отдельную collection fixture, уникальные container/database names, динамические host ports и pinned image. `MainContext` инициализируется только production migrations через `MigrateAsync`. Данные очищаются Respawn; migrations history сохраняется. Reuse, bind mounts, host backup files и скрытый skip при недоступном Docker запрещены.

Технический `MainDbUnitOfWorkScope` можно проверять изолированным fake `IMainDbContextSession`; реальные commit/rollback provider проверяются в SQL Server collection.
