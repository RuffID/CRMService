using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class IssueTypeService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<IssueTypeService> _logger = logger.CreateLogger<IssueTypeService>();

        public async Task<List<IssueType>?> GetIssueTypesFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/dictionaries/issues/types?api_token={okdeskSettings.Value.ApiToken}";

            List<IssueType>? typesFromApi = await request.GetRangeOfItems<IssueType>(link);

            if (typesFromApi != null && typesFromApi.Count != 0)
            {
                List<IssueType> types = [];
                LoopThroughAllElementsCollection(typesFromApi, types);
                return types;
            }
            return null;
        }

        private static void LoopThroughAllElementsCollection(List<IssueType> typesFromApi, List<IssueType> collectedType)
        {
            // Цикл проходит по всем полученным группам из API
            // Продолжать выполнение цикла пока не закончатся все полученные элементы
            foreach (var type in typesFromApi)
            {
                // Если элемент является группой, то пройтись по всем его вложенным элементам
                if (type.Type == "group" && type.Children != null && type.Children.Count != 0)
                    LoopThroughAllElementsCollection(type.Children, collectedType);

                // Если элемент является обычным типом без вложенности, то добавить его в новую коллекцию
                if (type.Type == "type")
                {
                    type.Id = default;
                    collectedType.Add(type);
                }
            }
        }

        public async Task<List<IssueType>?> GetIssueTypesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM issue_work_types ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(type => new IssueType
                {
                    Code = type.Field<string>("code"),
                    Name = type.Field<string>("name"),
                    Inner = type.Field<bool>("inner")
                }).ToList();
        }

        public async Task UpdateIssueTypesFromCloudApi()
        {
            List<IssueType>? types = await GetIssueTypesFromCloudApi();

            if (types == null || types.Count == 0)
                return;

            await unitOfWork.IssueType.CreateOrUpdate(types);

            await unitOfWork.SaveAsync();
        }

        public async Task UpdateIssueTypesFromCloudDb()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating issue types.", nameof(UpdateIssueTypesFromCloudDb));

            List<IssueType>? types = await GetIssueTypesFromCloudDb();

            if (types == null || types.Count == 0)
                return;

            await unitOfWork.IssueType.CreateOrUpdate(types);

            await unitOfWork.SaveAsync();

            _logger.LogInformation("[Method:{MethodName}] Issue types update completed.", nameof(UpdateIssueTypesFromCloudDb));
        }
    }
}