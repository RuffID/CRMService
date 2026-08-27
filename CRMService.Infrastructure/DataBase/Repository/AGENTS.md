# Infrastructure repositories и Unit of Work

Директория связывает Application data contracts с EFCoreLibrary и проектными DbContext.

`MainDbContextSession` владеет save/transaction операциями одного scoped `IAppDbContext<MainContext>`. `MainDbUnitOfWorkScope` и все сценарные Unit of Work делегируют этой сессии, поэтому repositories одного DI scope используют общий change tracker и транзакционную границу.

Правила:
- регистрировать session, scope, repositories и сценарные Unit of Work как scoped;
- не добавлять глобальный Unit of Work;
- EF Core query composition и provider-specific детали оставлять в Infrastructure;
- реализации предметных Application repository methods инкапсулируют predicates, includes и tracking; не возвращать эти детали обратно в Application-контракты;
- Okdesk source implementations являются read-only группировками и не имитируют отсутствующую транзакцию.
