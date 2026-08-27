# Тестовая зона

Каталог содержит отдельные test-проекты для production-слоёв и архитектурных проверок. Каждый проект ссылается только на разрешённый production-проект или набор проектов, указанный в его локальном `AGENTS.md`.

## Общие правила

- Использовать только xUnit v3 и Microsoft.Testing.Platform; общие test packages задаются в корневом `CRMService.Testing.props`.
- Не добавлять xUnit v2, coverlet и повторные ссылки на общие test packages в `.csproj`.
- Обычные тесты не обращаются к реальной сети, БД, Docker, production secrets, внешним сервисам и системным процессам.
- Каждый тест создаёт собственные данные, не зависит от порядка выполнения и не использует общие mutable fixtures.
- Ресурсы освобождаются через `Dispose`/`DisposeAsync`; ошибка подготовки fixture должна приводить к падению, а не к скрытому skip.
- Контейнерные тесты будущих этапов маркируются `[Trait("Dependency", "Docker")]`; контейнеры не переиспользуются между отдельными прогонами.

Быстрые тесты запускаются командой `dotnet test --project "tests\<Project>\<Project>.csproj" --filter-not-trait "Dependency=Docker"`. Docker-тесты запускаются отдельно с `--filter-trait "Dependency=Docker"` только в окружении, где Docker явно доступен.

CI запускает быстрые проекты отдельной matrix job, сохраняет TRX/Cobertura и запрещает снижение line/branch baseline из `tests/quality-gate-baseline.json`. Общие исключения coverage находятся в `tests/coverage.runsettings`; migrations, generated code и test assemblies не входят в baseline. Docker-набор выполняется отдельной job и не участвует в быстром gate.
