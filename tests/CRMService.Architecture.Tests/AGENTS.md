# CRMService.Architecture.Tests

Проект проверяет архитектурные границы решения. Разрешены ссылки на все пять production-проектов: Domain, Contracts, Application, Infrastructure и Web. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Тесты выполняют только статический анализ assembly и не используют реальную сеть, БД или Docker. Будущие контейнерные тесты сюда не добавляются; общий trait и команды фильтрации определены в `tests/AGENTS.md`.
