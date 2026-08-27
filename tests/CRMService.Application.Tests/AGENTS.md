# CRMService.Application.Tests

Проект содержит быстрые unit-тесты прикладной логики. Разрешена ссылка только на `CRMService.Application`; внешние зависимости заменяются контролируемыми doubles. Используются xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Реальная сеть, БД и Docker запрещены. Будущие контейнерные тесты сюда не добавляются; общий trait и команды фильтрации определены в `tests/AGENTS.md`.

Границы доступа к данным проверяются через узкие сценарные Unit of Work и read-only source-контракты; fake глобального Unit of Work не создавать.
