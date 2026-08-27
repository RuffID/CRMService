# CRMService.Infrastructure.IntegrationTests

Проект проверяет Infrastructure; разрешена ссылка только на `CRMService.Infrastructure`. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Обычные тесты не используют реальную сеть, БД и Docker. Контейнерные тесты будущего этапа маркируются `[Trait("Dependency", "Docker")]`: быстрые тесты выбираются через `--filter-not-trait "Dependency=Docker"`, Docker-тесты — через `--filter-trait "Dependency=Docker"`.

Технический `MainDbUnitOfWorkScope` можно проверять изолированным fake `IMainDbContextSession`; реальные commit/rollback provider проверяются только на контейнерном этапе.
