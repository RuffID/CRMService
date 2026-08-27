# Этап 7. Архитектурные проверки, CI и развитие покрытия

Status: Completed

## Реализовано

Архитектурные проверки покрывают направление `ProjectReference`, публичные DI aggregates, владение MVC/PageModel, DbContext и repository implementations, отсутствие Infrastructure implementations в конструкторах Application services, критические lifetimes/duplicates, порядок webhook handlers, заменяемость production registrations в Web test host и точный состав repositories сценарных Unit of Work.

Добавлен GitHub Actions workflow `.github/workflows/test-suite.yml`, запускаемый на `push`, `pull_request` и вручную. Fast matrix выполняет шесть test-проектов без Docker, публикует TRX/Cobertura и запрещает снижение line/branch baseline. Отдельный container job проверяет Docker, запускает Infrastructure tests с trait `Dependency=Docker`, публикует TRX/Cobertura и ограниченные container logs при сбое.

Coverage baseline хранится в `tests/quality-gate-baseline.json`, общие exclusions — в `tests/coverage.runsettings`. Из baseline исключены test assemblies, migrations, generated/designer code и Contracts DTO без поведения. Проверка baseline выполняется непосредственно внутри GitHub workflow без project scripts и дополнительных NuGet-пакетов. Имя baseline намеренно не начинается с `coverage`, чтобы файл не исключался общим правилом `.gitignore` для генерируемых coverage-отчётов.

## Результаты локальной проверки

| Набор | Тесты | Line | Branch |
|---|---:|---:|---:|
| Domain | 39 passed | 29.97% | 78.95% |
| Contracts | 11 passed | 61.02% | 0.00% |
| Application | 82 passed | 18.98% | 13.13% |
| Infrastructure fast | 43 passed | 12.94% | 2.42% |
| Web fast | 46 passed | 22.84% | 6.65% |
| Architecture | 18 passed | 3.19% | 0.90% |

Все шесть проектов восстановлены и собраны без warnings. Быстрые тесты выполнены без БД, Docker, production secrets, production jobs и внешней сети. Docker tests локально повторно не запускались; их отдельный CI-контур использует существующие trait, fixtures, timeout и cleanup strategy этапа 05.

## Architecture.Tests

Добавить правила, отражающие текущие границы решения:

- Domain не зависит от Application, Infrastructure и Web;
- Contracts не зависит от Application, Infrastructure и Web;
- Application не зависит от Infrastructure и Web;
- Infrastructure не зависит от Web;
- controllers/PageModels не используются из нижних слоёв;
- repository implementations и DbContext находятся только в Infrastructure;
- application services не зависят от конкретных Infrastructure-типов;
- старые глобальные `IUnitOfWork`/`IOkdeskUnitOfWork` отсутствуют, а сценарные Unit of Work не агрегируют repositories несвязанных областей;
- внешние HTTP и database реализации не проникают в Domain/Contracts.

Архитектурные тесты сначала должны зафиксировать фактические допустимые зависимости. Существующие нарушения оформить отдельными задачами, а не ослаблять правило неограниченными исключениями.

## CI-потоки

Разделить выполнение минимум на два jobs:

1. `unit-tests`: Domain, Contracts, Application, чистая часть Infrastructure, Web и Architecture с фильтром `--filter-not-trait "Dependency=Docker"`.
2. `container-tests`: Infrastructure container collections и выбранные Web end-to-end tests с фильтром `--filter-trait "Dependency=Docker"`; runner обязан иметь Docker.

Все команды выполнять через Microsoft.Testing.Platform, выбранный в `global.json`; отдельный проект передавать новым синтаксисом `dotnet test --project "tests\...\Project.csproj"`. Для обоих jobs публиковать TRX и coverage artifacts через `Microsoft.Testing.Extensions.TrxReport` и `Microsoft.Testing.Extensions.CodeCoverage`. Для container job при падении сохранять ограниченные логи контейнеров. Restore выполнять один раз с cache NuGet; тесты запускать без повторного restore после успешной сборки конкретных test-проектов.

## Порядок наращивания покрытия

1. Критические сценарии авторизации, синхронизации, webhook и migration/transactions.
2. Репозитории списков и отчётов с наиболее сложными запросами.
3. Ошибки внешних HTTP-сервисов и cancellation.
4. Web authentication, filters и middleware.
5. Остальные CRUD-сценарии по риску изменений.

Не устанавливать высокий глобальный процент сразу. После первой волны снять baseline по line/branch coverage, исключив migrations, generated code, DTO без поведения и designer-файлы. Затем вводить пороги поэтапно: сначала запрет снижения baseline, потом отдельные разумные пороги для Domain/Application и критических модулей.

## Definition of Done для каждого нового тестового набора

- Тест детерминирован и проходит независимо от порядка.
- Нет production secrets и внешней сети.
- Все disposable resources освобождаются.
- Docker test имеет timeout, trait `Dependency=Docker` и гарантированную cleanup strategy.
- Проверяется наблюдаемое поведение, а не детали реализации без причины.
- Исправление дефекта сопровождается regression test.
- Изменения тестовой архитектуры отражены в ближайшем `tests/AGENTS.md`.

## Итоговый критерий завершения плана

Все пять production-слоёв имеют собственный test-проект, архитектурные границы проверяются отдельно, быстрый suite не требует Docker, а SQL Server/PostgreSQL integration suites воспроизводимы локально и в CI и не оставляют ресурсов после завершения.
