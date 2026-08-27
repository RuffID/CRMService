# CRMService.Web.IntegrationTests

Проект проверяет HTTP- и host-контракты Web; разрешена ссылка только на `CRMService.Web`. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Обычные тесты не запускают production hosting и не используют реальную сеть, БД или Docker. Контейнерные тесты будущего этапа маркируются `[Trait("Dependency", "Docker")]`: быстрые тесты выбираются через `--filter-not-trait "Dependency=Docker"`, Docker-тесты — через `--filter-trait "Dependency=Docker"`.

`CrmWebApplicationFactory` использует environment `Testing`, in-memory configuration, ephemeral Data Protection и существующий публичный `ReportBackgroundService` как marker Web assembly. Factory после production registrations заменяет startup initializer, оба DbContext, authentication, webhook handlers и внешние HTTP services; production hosted jobs удаляются по точным implementation types.

Test factory не должна читать `Config/config.json`, открывать database connections или выполнять реальные HTTP-запросы. Внешний `IHttpApiClient` заменяется fail-fast proxy. Каждый тест освобождает `HttpClient` и factory; файловые тесты используют уникальный temporary content root и удаляют его в `finally`.
