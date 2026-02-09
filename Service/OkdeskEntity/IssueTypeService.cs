using CRMService.Abstractions.Database.Repository;
using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Models.ConfigClass;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Models.Request;
using CRMService.Models.Responses.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class IssueTypeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<IssueTypeService> logger)
    {
        public async Task<ServiceResult<List<TaskTypeDto>>> GetTypes(CancellationToken ct)
        {
            List<IssueType> types = await unitOfWork.IssueType.GetItemsByPredicateAsync(asNoTracking: true,
                include: t => t.Include(x => x.Group),
                ct: ct);

            return ServiceResult<List<TaskTypeDto>>.Ok(types.ToDto().ToList());
        }

        public async Task<ServiceResult<List<IssueTypeGroupDto>>> GetTypeGroups(CancellationToken ct)
        {
            List<IssueTypeGroup> types = await unitOfWork.IssueTypeGroup.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<IssueTypeGroupDto>>.Ok(types.ToDto().ToList());
        }

        public async Task<(List<IssueType> Types, List<IssueTypeGroup> TypeGroups)> GetIssueTypesFromCloudApi(CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/dictionaries/issues/types?api_token={okdeskSettings.Value.OkdeskApiToken}";

            List<IssueTypeResponse> root = await request.GetRangeOfItems<IssueTypeResponse>(link, ct: ct);
            List<IssueType> types = new List<IssueType>();
            List<IssueTypeGroup> groups = new List<IssueTypeGroup>();

            foreach (IssueTypeResponse type in root)
                TypeHandler(type, null, groups, types);

            return (types, groups);
        }

        public async Task<List<IssueType>> GetIssueTypesFromCloudDb(CancellationToken ct)
        {
            string sqlCommand = "SELECT * FROM issue_work_types ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand, ct);
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
            logger.LogInformation("[Method:{MethodName}] Starting to update issue types from API.", nameof(UpdateIssueTypesFromCloudApi));

            (List<IssueType> Types, List<IssueTypeGroup> Groups) = await GetIssueTypesFromCloudApi(ct);

            if (Types.Count == 0 && Groups.Count == 0)
                return;

            foreach (IssueType item in Types)
            {
                IssueType? existingTypes = await unitOfWork.IssueType.GetItemByIdAsync(item.Id, ct: ct);
                if (existingTypes == null)
                    unitOfWork.IssueType.Create(item);
                else
                    existingTypes.CopyData(item);
            }

            foreach (IssueTypeGroup item in Groups)
            {
                IssueTypeGroup? existingTypes = await unitOfWork.IssueTypeGroup.GetItemByIdAsync(item.Id, ct: ct);
                if (existingTypes == null)
                    unitOfWork.IssueTypeGroup.Create(item);
                else
                    existingTypes.CopyData(item);
            }

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateIssueTypesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue types.", nameof(UpdateIssueTypesFromCloudDb));

            List<IssueType> types = await GetIssueTypesFromCloudDb(ct);

            if (types.Count != 0)
            {
                foreach (IssueType item in types)
                {
                    IssueType? existingTypes = await unitOfWork.IssueType.GetItemByIdAsync(item.Id, ct: ct);
                    if (existingTypes == null)
                        unitOfWork.IssueType.Create(item);
                    else
                        existingTypes.CopyData(item);
                }

                await unitOfWork.SaveAsync(ct);
            }

            logger.LogInformation("[Method:{MethodName}] Issue types update completed.", nameof(UpdateIssueTypesFromCloudDb));
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
                IssueType type = new ()
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