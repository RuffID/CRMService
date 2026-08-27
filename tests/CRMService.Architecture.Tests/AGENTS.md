# CRMService.Architecture.Tests

Проект проверяет архитектурные границы решения. Разрешены ссылки на все пять production-проектов: Domain, Contracts, Application, Infrastructure и Web. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Тесты анализируют project references, production assemblies и `ServiceDescriptor` публичных DI aggregates; общий provider строится только с environment `Testing`, in-memory configuration, `ValidateScopes` и `ValidateOnBuild`. Реальная сеть, БД, production Data Protection directories и Docker не используются. Будущие контейнерные тесты сюда не добавляются; общий trait и команды фильтрации определены в `tests/AGENTS.md`.

Проверки защищают направление production-слоёв, единственность публичных DI aggregates, владение реализациями, lifetimes и уникальность критических registrations, порядок `IWebhookHandler`, отсутствие DI-заглушек в Domain/Contracts, а также отсутствие старых глобальных Unit of Work и прямых repository/Unit of Work зависимостей у контроллеров. Дополнительно фиксируются Web-владение MVC/PageModel, Infrastructure-владение DbContext/repository implementations, отсутствие Infrastructure implementations в конструкторах Application services и разрешённый состав repositories каждого сценарного Unit of Work. Внешняя architecture-test библиотека для этого не требуется.
