# Сценарные границы данных

Директория содержит Application-контракты repositories, общий технический `IUnitOfWorkScope`, шесть сценарных MainContext Unit of Work и три read-only Okdesk source-контракта.

Правила:
- сценарный Unit of Work открывает только repositories своей области и наследует `IUnitOfWorkScope`;
- Okdesk source не наследует scope, потому что текущие сценарии только читают облачную базу;
- не создавать глобальный агрегатор всех repositories;
- контракты repositories содержат только предметные методы и не должны содержать EF Core types, `IQueryable`, expressions, tracking/include-параметры или контекст;
- tracked и read-only операции различать именами предметных методов; query composition и provider-specific детали оставлять в Infrastructure.
