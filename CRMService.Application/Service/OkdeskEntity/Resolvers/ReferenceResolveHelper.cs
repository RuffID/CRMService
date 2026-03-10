using CRMService.Application.Service.Sync;
using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class ReferenceResolveHelper(EntitySyncService sync)
    {
        /// <summary>
        /// Выполняет поиск идентификатора, обновление справочника и повторную проверку.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<int> ResolveAsync<TKey>(
            // Ключ поиска текущей связанной сущности
            TKey lookupKey,
            // Функция локального поиска идентификатора
            Func<CancellationToken, Task<int?>> tryGetResolvedIdAsync,
            //Функция обновления справочника из внешнего источника
            Func<CancellationToken, Task> refreshAsync,
            // Функция построения ключа блокировки
            Func<TKey, string> createSyncKey,
            // Функцию построения текста предупреждения
            Func<TKey, string> buildNotFoundLogMessage,
            // Функция построения текста исключения
            Func<TKey, string> buildNotFoundExceptionMessage,
            ILogger logger,
            string methodName,
            CancellationToken ct)
            where TKey : notnull
        {
            // Выполняет первичный поиск идентификатора без обновления справочника.
            int? resolvedId = await tryGetResolvedIdAsync(ct);
            if (resolvedId.HasValue)
                return resolvedId.Value;

            await sync.RunExclusive(new ResolverSyncKey(createSyncKey(lookupKey)), async () =>
            {
                // Выполняет повторный поиск внутри блокировки на случай параллельного обновления.
                int? synchronizedResolvedId = await tryGetResolvedIdAsync(ct);
                // Проверяет успешный поиск после входа в блокировку.
                if (synchronizedResolvedId.HasValue)
                {
                    // Сохраняет найденный идентификатор во внешнюю переменную.
                    resolvedId = synchronizedResolvedId.Value;
                    // Завершает блок без обновления справочника.
                    return;
                }

                logger.LogWarning("[Method:{MethodName}] {Message}", methodName, buildNotFoundLogMessage(lookupKey));

                // Выполняет обновление справочника из источника данных.
                await refreshAsync(ct);

                // Выполняет повторный поиск после обновления справочника.
                synchronizedResolvedId = await tryGetResolvedIdAsync(ct);

                // Проверяет отсутствие сущности даже после обновления.
                if (!synchronizedResolvedId.HasValue)
                    throw new InvalidOperationException(buildNotFoundExceptionMessage(lookupKey));

                resolvedId = synchronizedResolvedId.Value;
            }, ct);

            return resolvedId!.Value;
        }

        // Внутренний тип ключа для использования в механизме блокировки
        private sealed class ResolverSyncKey(string key) : IEntity<string>
        {
            // Строковый идентификатор ключа блокировки
            public string Id { get; set; } = key;
        }
    }
}