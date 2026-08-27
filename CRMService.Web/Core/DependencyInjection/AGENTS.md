# Web dependency injection

Каталог содержит публичный aggregate `WebServiceCollectionExtensions.AddWeb` и внутренние регистрации, принадлежащие Web host.

Здесь находятся MVC/Razor Pages/SignalR, Smart authentication, Data Protection, middleware/filter registrations, Web coordination и file-backed services, startup initializer adapter и release-only hosted services.

Правила:
- не регистрировать Application или Infrastructure implementations;
- middleware pipeline, routing и запуск host оставлять в `Program.cs`;
- сохранять Smart selector, Cookie/JWT contracts и API 401/403 без redirect;
- Testing использует ephemeral Data Protection, остальные environments — filesystem keys;
- сохранять singleton/scoped/transient lifetimes Web services и release-only hosted registrations;
- `IStartupInitializer` остаётся Web adapter, чтобы test factory могла заменить его после production registrations.
