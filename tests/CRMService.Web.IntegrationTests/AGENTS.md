# CRMService.Web.IntegrationTests

Проект проверяет HTTP- и host-контракты Web; разрешена ссылка только на `CRMService.Web`. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Обычные тесты не запускают production hosting и не используют реальную сеть, БД или Docker. Контейнерные тесты будущего этапа маркируются `[Trait("Dependency", "Docker")]`: быстрые тесты выбираются через `--filter-not-trait "Dependency=Docker"`, Docker-тесты — через `--filter-trait "Dependency=Docker"`.
