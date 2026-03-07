using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace CRMService.Infrastructure.DataBase.Repository.OkdeskEntity
{
    public class OkdeskEquipmentRepository(
        IAppDbContext<OkdeskContext> dbContext,
        IGetItemByIdRepository<Equipment, int, OkdeskContext> getItemById,
        IGetItemByPredicateRepository<Equipment, OkdeskContext> getItemByPredicate) : IOkdeskEquipmentRepository
    {
        public Task<Equipment?> GetItemByIdAsync(int id, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemById.GetItemByIdAsync(id, asNoTracking, include, ct);

        public Task<Equipment?> GetItemByPredicateAsync(Expression<Func<Equipment, bool>> predicate, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemByPredicateAsync(predicate, asNoTracking, include, ct);

        public Task<List<Equipment>> GetItemsByPredicateAsync(Expression<Func<Equipment, bool>>? predicate = null, int skip = 0, int? take = null, bool asNoTracking = false, Func<IQueryable<Equipment>, IQueryable<Equipment>>? include = null, CancellationToken ct = default)
            => getItemByPredicate.GetItemsByPredicateAsync(predicate, skip, take, asNoTracking, include, ct);

        public async Task<List<Equipment>> GetSyncItemsAsync(int startId, int limit, CancellationToken ct = default)
        {
            List<EquipmentSyncProjection> rows = await dbContext.Set<Equipment>()
                .AsNoTracking()
                .Where(x => x.Id > startId)
                .OrderBy(x => x.Id)
                .Take(limit)
                .Select(x => new EquipmentSyncProjection
                {
                    Id = x.Id,
                    SerialNumber = x.SerialNumber,
                    InventoryNumber = x.InventoryNumber,
                    CompanyId = x.Company != null ? x.Company.Id : null,
                    MaintenanceEntityId = x.MaintenanceEntities != null ? x.MaintenanceEntities.Id : null,
                    KindId = x.KindId,
                    ManufacturerId = x.ManufacturerId,
                    ModelId = x.ModelId,
                    ParametersJson = EF.Property<string?>(x, "ParametersJson")
                })
                .ToListAsync(ct);

            return rows
                .Select(x => new Equipment
                {
                    Id = x.Id,
                    SerialNumber = x.SerialNumber,
                    InventoryNumber = x.InventoryNumber,
                    CompanyId = x.CompanyId,
                    MaintenanceEntitiesId = x.MaintenanceEntityId,
                    KindId = x.KindId,
                    ManufacturerId = x.ManufacturerId,
                    ModelId = x.ModelId,
                    Parameters = HandleParameters(x.Id, x.ParametersJson)
                })
                .ToList();
        }

        private static List<EquipmentParameter> HandleParameters(int equipmentId, string? parameters)
        {
            List<EquipmentParameter> result = new();
            if (string.IsNullOrEmpty(parameters))
                return result;

            using JsonDocument document = JsonDocument.Parse(parameters);
            JsonElement root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    string? valueStr = property.Value.ValueKind switch
                    {
                        JsonValueKind.Null => null,
                        JsonValueKind.String => property.Value.GetString(),
                        _ => property.Value.GetRawText()
                    };

                    if (string.IsNullOrEmpty(valueStr))
                        continue;

                    result.Add(new EquipmentParameter
                    {
                        Code = property.Name,
                        Value = valueStr,
                        EquipmentId = equipmentId
                    });
                }

                return result;
            }

            result.Add(new EquipmentParameter
            {
                Code = null,
                Value = root.ValueKind == JsonValueKind.Array || root.ValueKind == JsonValueKind.Object
                    ? root.GetRawText()
                    : ConvertScalarJsonElement(root),
                EquipmentId = equipmentId
            });

            return result;
        }

        private static object? ConvertScalarJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt64(out long int64Value) => int64Value,
                JsonValueKind.Number when element.TryGetDecimal(out decimal decimalValue) => decimalValue,
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => element.GetRawText()
            };
        }

        private class EquipmentSyncProjection
        {
            public int Id { get; set; }
            public string? SerialNumber { get; set; }
            public string? InventoryNumber { get; set; }
            public int? CompanyId { get; set; }
            public int? MaintenanceEntityId { get; set; }
            public int? KindId { get; set; }
            public int? ManufacturerId { get; set; }
            public int? ModelId { get; set; }
            public string? ParametersJson { get; set; }
        }
    }
}
