# CRMService.Contracts.Tests

Проект проверяет контракты результатов, DTO и сериализации. Разрешена ссылка только на `CRMService.Contracts`; тесты используют xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Реальная сеть, БД и Docker запрещены. Будущие контейнерные тесты сюда не добавляются; общий trait и команды фильтрации определены в `tests/AGENTS.md`.

JSON-контракты Razor Pages и API проверять с web defaults `System.Text.Json`. Не фиксировать DTO полным snapshot: проверять round-trip, nullable-поля и только обязательные либо исторически чувствительные JSON-имена.
