using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class EmployeeService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, ICompanyDirectoryUnitOfWork unitOfWork, IOkdeskCompanyDirectorySource okdeskUnitOfWork, IOkdeskEntityRequestService request, EntitySyncService sync, ILogger<EmployeeService> logger)
    {
        public Task<Employee?> GetEmployeeAsync(int id, CancellationToken ct = default) =>
            unitOfWork.Employee.GetItemByIdReadOnlyAsync(id, ct);

        public Task<List<EmployeeGroup>> GetEmployeeGroupConnectionsAsync(CancellationToken ct = default) =>
            unitOfWork.EmployeeGroup.GetItemsReadOnlyAsync(ct);

        public Task<List<Employee>> GetEmployeesByGroupAsync(int groupId, CancellationToken ct = default) =>
            unitOfWork.Employee.GetWithGroupsReadOnlyAsync([groupId], includeInactive: true, ct);

        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<EmployeeDto>>> GetEmployees(List<int>? groupIds = null, CancellationToken ct = default)
            => await GetEmployeesAsync(groupIds, includeInactive: false, ct);

        public async Task<ServiceResult<List<EmployeeDto>>> GetEmployeesAsync(List<int>? groupIds = null, bool includeInactive = false, CancellationToken ct = default)
        {
            List<Employee> employees = await unitOfWork.Employee.GetWithGroupsReadOnlyAsync(groupIds, includeInactive, ct);

            List<EmployeeDto> items = employees
                .OrderBy(employee => employee.LastName)
                .ThenBy(employee => employee.FirstName)
                .ThenBy(employee => employee.Patronymic)
                .ThenBy(employee => employee.Id)
                .ToDto()
                .ToList();

            return ServiceResult<List<EmployeeDto>>.Ok(items);
        }

        public async Task<ServiceResult<List<LookupOptionDto>>> GetEmployeeLookupAsync(LookupListRequest requestModel, CancellationToken ct = default, bool activeOnly = false)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);

            List<Employee> employees = await unitOfWork.Employee.SearchReadOnlyAsync(normalizedSearch, activeOnly, ct);

            IEnumerable<Employee> orderedEmployees = normalizedSearch == null
                ? employees.OrderBy(employee => employee.Id)
                : employees
                    .OrderBy(employee => GetSearchRank(employee, normalizedSearch))
                    .ThenBy(employee => employee.LastName)
                    .ThenBy(employee => employee.FirstName)
                    .ThenBy(employee => employee.Patronymic)
                    .ThenBy(employee => employee.Id);

            List<LookupOptionDto> items = orderedEmployees
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(employee => new LookupOptionDto
                {
                    Id = employee.Id,
                    Text = FormatEmployeeName(employee)
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        public async IAsyncEnumerable<List<Employee>> GetEmployeesFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/employees/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Employee> employees in request.GetAllItemsAsync<Employee>(link, startIndex: 0, limit, ct: ct))
                yield return employees;
        }

        private async Task<List<Employee>> GetEmployeesFromCloudDb(CancellationToken ct)
        {
            List<Employee> employees = await okdeskUnitOfWork.Employee.GetEmployeesReadOnlyAsync(ct);

            return employees.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateEmployeesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employees from API.", nameof(UpdateEmployeesFromCloudApi));

            await foreach (List<Employee> employees in GetEmployeesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Employee employee in employees)
                {
                    await sync.RunExclusive(employee, async () =>
                    {
                        Employee? existingEmployee = await unitOfWork.Employee.GetItemByIdAsync(employee.Id, ct);
                        if (existingEmployee == null)
                            unitOfWork.Employee.Create(employee);
                        else
                            existingEmployee.CopyData(employee);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update employees completed.", nameof(UpdateEmployeesFromCloudApi));
        }

        public async Task UpdateEmployeesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update employees from DB.", nameof(UpdateEmployeesFromCloudDb));

            List<Employee> employees = await GetEmployeesFromCloudDb(ct);

            if (employees.Count != 0)
            {
                foreach (Employee employee in employees)
                {
                    await sync.RunExclusive(employee, async () =>
                    {
                        Employee? existingEmployee = await unitOfWork.Employee.GetItemByIdAsync(employee.Id, ct);
                        if (existingEmployee == null)
                            unitOfWork.Employee.Create(employee);
                        else
                            existingEmployee.CopyData(employee);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update employees completed.", nameof(UpdateEmployeesFromCloudDb));
        }

        private static ServiceResult ValidateLookupRequest(LookupListRequest request)
        {
            if (request.Offset < 0)
                return ServiceResult.Fail(400, "Смещение не может быть отрицательным.");

            if (request.Limit == 0)
                request.Limit = DEFAULT_LOOKUP_LIMIT;

            if (request.Limit <= 0 || request.Limit > 100)
                return ServiceResult.Fail(400, "Лимит должен быть в диапазоне от 1 до 100.");

            return ServiceResult.Ok();
        }

        private static string? NormalizeSearch(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim();
            int nonWhitespaceCount = normalized.Count(character => !char.IsWhiteSpace(character));
            return nonWhitespaceCount >= 2 ? normalized : null;
        }

        private static int GetSearchRank(Employee employee, string search)
        {
            string fullName = FormatEmployeeName(employee);

            if (fullName.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (!string.IsNullOrWhiteSpace(employee.LastName) && employee.LastName.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (!string.IsNullOrWhiteSpace(employee.FirstName) && employee.FirstName.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 2;

            if (fullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 3;

            return 4;
        }

        private static string FormatEmployeeName(Employee employee)
        {
            string[] parts = new[]
            {
                employee.LastName ?? string.Empty,
                employee.FirstName ?? string.Empty,
                employee.Patronymic ?? string.Empty
            };

            string fullName = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
            return string.IsNullOrWhiteSpace(fullName) ? $"#{employee.Id}" : fullName;
        }
    }
}
