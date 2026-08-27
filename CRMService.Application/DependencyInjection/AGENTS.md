# Application dependency injection

Каталог содержит публичный aggregate `ApplicationServiceCollectionExtensions.AddApplication` и внутренние группы регистраций прикладных сервисов.

Правила:
- регистрировать здесь только реализации из `CRMService.Application`;
- не регистрировать DbContext, repositories, Unit of Work implementations, HTTP clients, JWT/Telegram infrastructure и Web host services;
- сохранять исходные lifetimes сервисов;
- сохранять порядок `IWebhookHandler`: Issue, Company, MaintenanceEntity, Equipment;
- новые группы добавлять в `AddApplication` в явном порядке и только при наличии реальной функциональной границы.
