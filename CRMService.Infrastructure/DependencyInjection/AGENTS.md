# Infrastructure dependency injection

Каталог содержит публичный aggregate `InfrastructureServiceCollectionExtensions.AddInfrastructure` и внутренние группы registrations инфраструктурного слоя.

Здесь регистрируются options binding/validation, оба DbContext, EF adapters, общая scoped MainContext session, сценарные Unit of Work, repositories, read-only Okdesk sources, внешние HTTP integrations, JWT и database services.

Правила:
- не регистрировать Application implementations, Web middleware, filters, hosted services или startup initializer;
- DbContext, adapters, session, repositories, sources и Unit of Work сохранять scoped;
- все MainContext repositories внутри scope должны использовать один change tracker через `IMainDbContextSession`;
- не объединять сценарные Unit of Work и read-only Okdesk sources в глобальный агрегатор;
- не дублировать typed HTTP clients, options configuration или DbContext registrations;
- сохранять `ValidateOnStart`, SQL Server/Npgsql providers и HTTP timeout 180 секунд.
