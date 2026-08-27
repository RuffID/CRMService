# Этап 7. Архитектурные проверки, CI и развитие покрытия

Status: Not Started

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
