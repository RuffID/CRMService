# Этап 1. Основа и структура проектов

## 1. Создать проекты

Создать шесть SDK-style test-проектов из целевой структуры. Каждый `.csproj` должен явно содержать `<TargetFramework>net10.0</TargetFramework>`, nullable, implicit usings и `RootNamespace`, совпадающий с именем проекта. `TargetFramework` задаёт целевую платформу сборки и выполнения тестовой assembly.

По образцу AquaByte Ledger вынести общие свойства и пакеты в корневой `CRMService.Testing.props`. Каждый test-проект импортирует его после основного `PropertyGroup`:

```xml
<Import Project="..\..\CRMService.Testing.props" />
```

В `CRMService.Testing.props` задать `IsPackable=false`, `IsTestProject=true` и `DefaultItemExcludes` для всех вложенных `bin`, `obj` и `artifacts`. Там же централизованно подключить:

- `Microsoft.NET.Test.Sdk` версии `18.9.0`;
- `Microsoft.Testing.Extensions.CodeCoverage` версии `18.9.0` с `PrivateAssets=all`;
- `Microsoft.Testing.Extensions.TrxReport` версии `2.3.3` с `PrivateAssets=all`;
- `xunit.v3` версии `4.0.0`;
- `xunit.runner.visualstudio` версии `4.0.0` с `PrivateAssets=all` и явным `IncludeAssets`.

Эти версии соответствуют проверенной конфигурации AquaByte Ledger на .NET 10. Перед реализацией выполнить restore только test-проектов и подтвердить их доступность из текущих NuGet sources; при несовместимости обновлять согласованным комплектом, не откатываться на xUnit v2. Специализированные пакеты добавлять только в соответствующем проекте.

## 2. Настроить Microsoft.Testing.Platform

Создать корневой `global.json` с закреплённой установленной версией .NET 10 SDK, `rollForward=latestPatch` и настройкой:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

Поле `test.runner` выбирает платформу запуска тестов. Оно не заменяет `TargetFramework`: без него .NET 10 `dotnet test` может использовать несовместимый старый VSTest target, тогда как test-проекты построены на xUnit v3/Microsoft.Testing.Platform.

Добавить `CRMService.Testing.props` и `global.json` как корневые `<File>` в `CRMService.slnx`, а созданные проекты — внутрь solution folder `/tests/` по тому же принципу, что в AquaByte Ledger.

## 3. Настроить ссылки между проектами

- Domain.Tests → Domain.
- Contracts.Tests → Contracts.
- Application.Tests → Application; прямые ссылки на Contracts/Domain добавлять только если тестовый код использует их типы.
- Infrastructure.IntegrationTests → Infrastructure; нижние слои будут доступны транзитивно, прямые ссылки добавлять только при необходимости.
- Web.IntegrationTests → Web.
- Architecture.Tests → все пять production-проектов.

Не добавлять ссылок из production-проектов на `tests`.

## 4. Зафиксировать правила тестовой зоны

Создать `tests/AGENTS.md` со следующими правилами:

- разделять unit, integration и container suites;
- использовать только `xunit.v3` и Microsoft.Testing.Platform;
- запускать отдельный проект новым синтаксисом `dotnet test --project "tests\<Project>\<Project>.csproj"`;
- выбирать Docker-тесты через `--filter-trait "Dependency=Docker"`, а быстрые — через `--filter-not-trait "Dependency=Docker"`;
- запрещать реальную сеть в обычных тестах;
- не использовать общие статические mutable fixtures;
- каждый тест сам создаёт данные и не зависит от порядка запуска;
- контейнеры не переиспользуются между отдельными запусками;
- временные каталоги, streams, hosts, scopes и контейнеры освобождаются через `Dispose`/`DisposeAsync`;
- падение подготовки fixture считается падением тестов, а не причиной скрытого skip.

## 5. Добавить общие служебные типы без преждевременной абстракции

Сначала держать builders/fakes внутри конкретного проекта. Выносить общий `tests/CRMService.Testing` только если один и тот же код действительно нужен как минимум трём проектам. Предполагаемые локальные помощники:

- builders доменных сущностей;
- фиксированные часы/идентификаторы, если production-код получит соответствующие abstractions;
- fake HTTP handler;
- фабрики валидных Options;
- capture logger только для сценариев, где логирование является частью проверяемого поведения.

## Критерии завершения

- Все test-проекты находятся в `tests` и добавлены в solution.
- Все test-проекты импортируют один `CRMService.Testing.props`, используют `xunit.v3` и обнаруживаются Microsoft.Testing.Platform.
- `global.json` закрепляет .NET 10 SDK и содержит `test.runner=Microsoft.Testing.Platform`.
- В solution отсутствуют package references на xUnit v2 и альтернативный coverage collector вне Microsoft.Testing.Platform.
- Пустые проекты восстанавливаются и собираются независимо.
- Обычный unit-прогон не требует Docker.
- Docker suite выбирается отдельным MTP trait filter.
