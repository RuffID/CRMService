# План построения многоуровневой тестовой системы

## Цель

Создать в `tests` отдельные тестовые проекты для всех проектов решения и отделить быстрые тесты от проверок, которым нужны ASP.NET Core host и реальные СУБД. Инфраструктурные тесты должны работать с SQL Server и PostgreSQL в Docker-контейнерах и гарантированно освобождать контейнеры после выполнения.

На момент составления плана папка `tests` пуста. В решении находятся пять production-проектов на `net10.0`: Domain, Contracts, Application, Infrastructure и Web. `MainContext` использует SQL Server и имеет EF Core migrations; `OkdeskContext` использует PostgreSQL и отдельных migrations не имеет.

## Целевая структура

```text
CRMService.Testing.props
global.json
tests/
  AGENTS.md
  CRMService.Domain.Tests/
  CRMService.Contracts.Tests/
  CRMService.Application.Tests/
  CRMService.Infrastructure.IntegrationTests/
  CRMService.Web.IntegrationTests/
  CRMService.Architecture.Tests/
```

Проекты должны быть добавлены в solution и сгруппированы в solution folder `tests`. Каждый проект ссылается только на проверяемый слой и действительно необходимые нижележащие проекты.

## Общие решения

- Основной фреймворк: только `xunit.v3`; пакеты xUnit v2 не добавлять даже временно.
- Общие test properties и версии runner-пакетов задаются корневым `CRMService.Testing.props`, который импортирует каждый test-проект.
- Корневой `global.json` задаёт `test.runner` равным `Microsoft.Testing.Platform`; это включает новый test runner для .NET 10 и xUnit v3 вместо VSTest target.
- Каждый test-проект явно задаёт `<TargetFramework>net10.0</TargetFramework>`: это целевая платформа компиляции и выполнения тестовой assembly, а не настройка runner.
- Проверки: встроенные `Assert`; дополнительную assertion-библиотеку вводить только при реальной необходимости.
- Подмены зависимостей: NSubstitute либо собственные небольшие fake-объекты. Не создавать один универсальный mock для всего `IUnitOfWork`.
- Покрытие и TRX: `Microsoft.Testing.Extensions.CodeCoverage` и `Microsoft.Testing.Extensions.TrxReport`, совместимые с Microsoft.Testing.Platform; пороги вводить после появления базового покрытия.
- Имена тестов: `Method_Scenario_ExpectedResult`; структура каталогов повторяет production-код.
- Unit-тесты не обращаются к сети, файловой системе, системным процессам, Docker или реальной БД.
- Тесты внешнего HTTP используют контролируемый `HttpMessageHandler`/fake `IHttpApiClient`, без исходящих запросов.
- Docker-тесты помечаются `Trait("Dependency", "Docker")`; быстрый и контейнерный наборы выбираются командами Microsoft.Testing.Platform `--filter-not-trait "Dependency=Docker"` и `--filter-trait "Dependency=Docker"`.
- EF Core InMemory не использовать для проверки репозиториев и конфигураций: он не воспроизводит поведение SQL Server/PostgreSQL.

## Порядок реализации

1. [Основа и структура проектов](01-foundation.md).
2. [Декомпозиция Unit of Work](02-unit-of-work-decomposition.md).
3. [Domain и Contracts](03-domain-and-contracts.md).
4. [Application](04-application.md).
5. [Infrastructure и Testcontainers](05-infrastructure-containers.md).
6. [Web integration](06-web-integration.md).
7. [Архитектурные проверки, CI и развитие покрытия](07-quality-gates-and-rollout.md).

## Граница текущей задачи

Этот каталог содержит только план. Тестовые проекты, production-рефакторинг для тестируемости, Docker-конфигурация и сами тесты на этом этапе не создаются.
