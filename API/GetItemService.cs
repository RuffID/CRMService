using CRMService.Interfaces.Api;
using CRMService.Interfaces.Entity;
using CRMService.Models.ConfigClass;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace CRMService.API
{
    public class GetItemService(IRequestService client, ILoggerFactory logger)
    {
        private readonly ILogger<GetItemService> _logger = logger.CreateLogger<GetItemService>();

        public async IAsyncEnumerable<List<T>> GetAllItems<T>(string link, long startIndex, long limit, long pageNubmer = 0, [CallerMemberName] string caller = "")
        {
            while (true)
            {
                List<T>? collection = await GetRangeOfItems<T>(link, startIndex, limit, pageNubmer, caller);

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

        public async Task<List<T>?> GetRangeOfItems<T>(string link, long startIndex = 0, long limit = 0, long pageNubmer = 0, [CallerMemberName] string caller = "")
        {
            // Задержка чтобы не посылать запросы слишком часто
            await Task.Delay(2000);

            if (limit > LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API) 
                limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API;

            if (limit != 0 || startIndex != 0)
                link += $"&page[size]={limit}&page[direction]=forward&page[from_id]={startIndex}";

            if (pageNubmer != 0)
                link += $"&page[number]={pageNubmer}";

            string? response = await client.SendGet(link);
            if (string.IsNullOrEmpty(response) || response == "[]")
                return null;

            try
            {
                return JsonConvert.DeserializeObject<List<T>>(response);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Method: {MethodName}, Caller: {Caller}", nameof(GetRangeOfItems), caller);
                return null; 
            }
        }

        public async Task<T?> GetItem<T>(string link, [CallerMemberName] string caller = "")
        {
            string? response = await client.SendGet(link);

            if (string.IsNullOrEmpty(response) || response == "[]")
                return default;

            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Method: {MethodName}, Caller: {Caller}", nameof(GetItem), caller);
                return default; 
            }
        }           
        
    }
}