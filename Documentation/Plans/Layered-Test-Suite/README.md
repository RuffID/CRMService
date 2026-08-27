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

| Этап | Статус | План |
|---|---|---|
| 01 | Completed | [Основа и структура проектов](01-foundation.md) |
| 02 | Completed | [Декомпозиция Unit of Work](02-unit-of-work-decomposition.md) |
| 03 | Completed | [Domain и Contracts](03-domain-and-contracts.md) |
| 04 | Completed | [Application](04-application.md) |
| 05 | Completed | [Infrastructure и Testcontainers](05-infrastructure-containers.md) |
| 06 | Completed | [Web integration](06-web-integration.md) |
| 07 | Not Started | [Архитектурные проверки, CI и развитие покрытия](07-quality-gates-and-rollout.md) |

## Жизненный цикл этапов

- `Not Started` — работы не начинались.
- `In Progress` — этап выполняется.
- `Blocked` — продолжение невозможно без внешнего решения.
- `Completed` — все работы и проверки этапа завершены.

Перед первым изменением этап переводится в `In Progress`. `Completed` устанавливается только после выполнения всех критериев и проверок; наличие созданного кода без проверки не считается завершением. При частичном выполнении статус остаётся `In Progress`. При переводе в `Blocked` в файле этапа нужно указать причину блокировки и перечень незавершённых работ.

## Граница этапа 01

Этап создаёт только основу тестовой системы и smoke tests для проверки discovery. Production-рефакторинг, предметные тесты, Docker-конфигурация и специализированные зависимости выполняются на следующих этапах.
