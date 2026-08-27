# CI workflows

Каталог содержит GitHub Actions для многоуровневой тестовой системы.

Правила:
- быстрые и Docker-тесты выполняются отдельными jobs;
- каждый test-проект восстанавливается один раз, затем собирается и тестируется с `--no-restore`;
- быстрые jobs публикуют TRX и Cobertura и проверяют `tests/quality-gate-baseline.json` непосредственно в workflow без project scripts;
- Docker job сначала проверяет доступность Docker, запускает только trait `Dependency=Docker` и публикует ограниченные container logs при сбое;
- не добавлять production secrets, deployment и запуск CRMService host;
- версии общих test packages остаются в `CRMService.Testing.props`.
