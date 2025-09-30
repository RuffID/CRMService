using CRMService.Interfaces.Api;
using CRMService.Interfaces.Entity;
using CRMService.Models.Constants;
using System.Runtime.CompilerServices;

namespace CRMService.API
{
    public class GetOkdeskEntityService(IHttpApiClient client)
    {
        public async IAsyncEnumerable<List<T>> GetAllItems<T>(string link, long startIndex, long limit, long pageNubmer = 0, [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                List<T>? collection = await GetRangeOfItems<T>(link, startIndex, limit, pageNubmer, ct);

                if (collection == null || collection.Count == 0)
                    yield break;
                
                if (collection.Last() is IEntity<int> entity)
                    startIndex = entity.Id + 1;

                if (pageNubmer != 0)
                    pageNubmer++;

                yield return collection;

                if (collection.Count < limit)
                    yield break;
            }
        }

        public async Task<List<T>> GetRangeOfItems<T>(string link, long startIndex = 0, long limit = 0, long pageNubmer = 0, CancellationToken ct = default)
        {
            // Задержка чтобы не посылать запросы слишком часто
            await Task.Delay(2000, ct);

            if (limit > LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API) 
                limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API;

            if (limit != 0 || startIndex != 0)
                link += $"&page[size]={limit}&page[direction]=forward&page[from_id]={startIndex}";

            if (pageNubmer != 0)
                link += $"&page[number]={pageNubmer}";

            return await client.GetAsync<List<T>>(link, ct: ct) ?? new();
        }

        public async Task<T?> GetItem<T>(string link, CancellationToken ct = default)
        {
            return await client.GetAsync<T>(link, ct: ct);    
        }        
    }
}