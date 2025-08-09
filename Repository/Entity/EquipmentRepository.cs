using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Dto.Entity;

namespace CRMService.Repository.Entity
{
    public class EquipmentRepository(CrmEntitiesContext context, ILoggerFactory logger) : IEquipmentRepository
    {
        private readonly ILogger<EquipmentRepository> _logger = logger.CreateLogger<EquipmentRepository>();

        public async Task<IEnumerable<Equipment>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Equipment.AsNoTracking().Where(e => e.Id >= startIndex).OrderBy(e => e.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving equipment list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<IEnumerable<EquipmentDto>?> GetEquipmentsByMaintenanceEntity(int maintenanceId)
        {
            try
            {
                return await context.Equipment
                    .AsNoTracking()
                    .Where(e => e.MaintenanceEntitiesId == maintenanceId)
                    .Select(e => new EquipmentDto
                    {
                        Id = e.Id,
                        Serial_number = e.SerialNumber,
                        Inventory_number = e.InventoryNumber,
                        Kind = e.Kind != null ? new KindDto
                        {
                            Id = e.Kind.Id,
                            Name = e.Kind.Name,
                            Code = e.Kind.Code
                        } : null,
                        Manufacturer = e.Manufacturer != null ? new ManufacturerDto
                        {
                            Id = e.Manufacturer.Id,
                            Name = e.Manufacturer.Name,
                            Code = e.Manufacturer.Code
                        } : null,
                        Model = e.Model != null ? new ModelDto
                        {
                            Id = e.Model.Id,
                            Name = e.Model.Name,
                            Code = e.Model.Code
                        } : null
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving equipment list by maintenance entity.", nameof(GetEquipmentsByMaintenanceEntity));
                return null;
            }
        }

        public async Task<IEnumerable<EquipmentDto>?> GetEquipmentsByCompany(int companyId)
        {
            try
            {
                return await context.Equipment
                    .AsNoTracking()
                    .Where(e => e.CompanyId == companyId)
                    .Select(e => new EquipmentDto
                    {
                        Id = e.Id,
                        Serial_number = e.SerialNumber,
                        Inventory_number = e.InventoryNumber,
                        Kind = e.Kind != null ? new KindDto
                        {
                            Id = e.Kind.Id,
                            Name = e.Kind.Name,
                            Code = e.Kind.Code
                        } : null,
                        Manufacturer = e.Manufacturer != null ? new ManufacturerDto
                        {
                            Id = e.Manufacturer.Id,
                            Name = e.Manufacturer.Name,
                            Code = e.Manufacturer.Code
                        } : null,
                        Model = e.Model != null ? new ModelDto
                        {
                            Id = e.Model.Id,
                            Name = e.Model.Name,
                            Code = e.Model.Code
                        } : null
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving equipment list by company.", nameof(GetEquipmentsByCompany));
                return null;
            }
        }

        public async Task<EquipmentDto?> GetEquipmentById(int equipmentId)
        {
            try
            {
                return await context.Equipment
                    .AsNoTracking()
                    .Where(e => e.Id == equipmentId)
                    .Select(e => new EquipmentDto
                    {
                        Id = e.Id,
                        Serial_number = e.SerialNumber,
                        Inventory_number = e.InventoryNumber,
                        Kind = e.Kind != null ? new KindDto
                        {
                            Id = e.Kind.Id,
                            Name = e.Kind.Name,
                            Code = e.Kind.Code
                        } : null,
                        Manufacturer = e.Manufacturer != null ? new ManufacturerDto
                        {
                            Id = e.Manufacturer.Id,
                            Name = e.Manufacturer.Name,
                            Code = e.Manufacturer.Code
                        } : null,
                        Model = e.Model != null ? new ModelDto
                        {
                            Id = e.Model.Id,
                            Name = e.Model.Name,
                            Code = e.Model.Code
                        } : null
                    })
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving equipment by id.", nameof(GetEquipmentById));
                return null;
            }
        }

        public async Task<Equipment?> GetItem(Equipment item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Equipment.FirstOrDefaultAsync(e => e.Id == item.Id);

                return await context.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving equipment.", nameof(GetItem));
                return null;
            }
        }

        public void Update(Equipment oldItem, Equipment newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Equipment item)
        {
            context.Equipment.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Equipment> items)
        {
            foreach (var item in items)
            {
                var existingItem = await GetItem(item);

                if (existingItem == null)
                    Create(item);
                else
                    Update(existingItem, item);
            }
        }
    }
}
