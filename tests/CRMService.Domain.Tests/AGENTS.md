# CRMService.Domain.Tests

Проект проверяет чистое поведение и инварианты Domain. Разрешена ссылка только на `CRMService.Domain`; тесты используют xUnit v3 и Microsoft.Testing.Platform через `CRMService.Testing.props`.

Реальная сеть, БД и Docker запрещены. Будущие контейнерные тесты сюда не добавляются; общий trait и команды фильтрации определены в `tests/AGENTS.md`.
