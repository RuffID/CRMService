# CRMService.Application.Tests

Проект содержит быстрые unit-тесты прикладной логики. Разрешена ссылка только на `CRMService.Application`; внешние зависимости заменяются контролируемыми doubles. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Реальная сеть, БД и Docker запрещены. Будущие контейнерные тесты сюда не добавляются; общий trait и команды фильтрации определены в `tests/AGENTS.md`.
