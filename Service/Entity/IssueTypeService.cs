using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Models.Request;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class IssueTypeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<IssueTypeService> _logger = logger.CreateLogger<IssueTypeService>();

        public async Task<(List<IssueType> Types, List<IssueTypeGroup> TypeGroups)> GetIssueTypesFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/dictionaries/issues/types?api_token={okdeskSettings.Value.OkdeskApiToken}";

            List<IssueTypeResponse> root = await request.GetRangeOfItems<IssueTypeResponse>(link);
            List<IssueType> types = new List<IssueType>();
            List<IssueTypeGroup> groups = new List<IssueTypeGroup>();
            
            foreach (IssueTypeResponse type in root)
                TypeHandler(type, null, groups, types);

            return (types, groups);
        }
        
        public async Task<List<IssueType>> GetIssueTypesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM issue_work_types ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new List<IssueType>();

            return table.AsEnumerable().
                Select(type => new IssueType
                {
                    Id = type.Field<int>("id"),
                    Code = type.Field<string>("code") ?? "",
                    Name = type.Field<string>("name") ?? string.Empty,
                    IsInner = type.Field<bool>("inner")
                }).ToList();
        }

        public async Task UpdateIssueTypesFromCloudApi(CancellationToken ct)
        {
            (List<IssueType> Types, List<IssueTypeGroup> Groups) = await GetIssueTypesFromCloudApi();

            if (Types.Count == 0 && Groups.Count == 0)
                return;

            await unitOfWork.ExecuteInTransaction(async () =>
            {
                await unitOfWork.IssueTypeGroup.Upsert(Groups, ct);

                await unitOfWork.IssueType.Upsert(Types, ct);
            }, ct);
        }

        public async Task UpdateIssueTypesFromCloudDb(CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating issue types.", nameof(UpdateIssueTypesFromCloudDb));

            List<IssueType> types = await GetIssueTypesFromCloudDb();

            if (types.Count != 0)
            {
                await unitOfWork.IssueType.Upsert(types, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Issue types update completed.", nameof(UpdateIssueTypesFromCloudDb));
        }

        private void TypeHandler(IssueTypeResponse node, int? parentGroupId, List<IssueTypeGroup> groups, List<IssueType> types)
        {
            bool isGroup = string.Equals(node.Type, "group", StringComparison.OrdinalIgnoreCase);

            if (isGroup)
            {
                IssueTypeGroup group = new IssueTypeGroup
                {
                    Id = node.Id,
                    Code = node.Code,
                    Name = node.Name,
                    ParentGroupId = parentGroupId
                };
                groups.Add(group);

                if (node.Children != null)
                {
                    foreach (IssueTypeResponse child in node.Children)
                        TypeHandler(child, node.Id, groups, types);
                }
            }
            else
            {
                IssueType type = new IssueType
                {
                    Id = node.Id,
                    Code = node.Code,
                    Name = node.Name,
                    IsDefault = node.Default,
                    IsInner = node.Inner,
                    AvailableForClient = node.AvailableForClient,
                    GroupId = parentGroupId
                };
                types.Add(type);
            }
        }
    }
}