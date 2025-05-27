using Microsoft.EntityFrameworkCore;
using CRMService.Dto;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class EquipmentRepository(CRMEntitiesContext context, ILoggerFactory logger) : IEquipmentRepository
    {
        private readonly ILogger<EquipmentRepository> _logger = logger.CreateLogger<EquipmentRepository>();

        public async Task<IEnumerable<Equipment>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Equipment.AsNoTracking().OrderBy(e => e.Id).Where(e => e.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment list.");
                return null;
            }
        }

        public async Task<IEnumerable<EquipmentDto>?> GetEquipmentsByMaintenanceEntity(int maintenanceId)
        {
            try
            {
                return await (from equipment in context.Equipment
                              join kind in context.Kinds on equipment.KindId equals kind.Id into kindGroup
                              from kind in kindGroup.DefaultIfEmpty()
                              join manufacturers in context.Manufacturers on equipment.ManufacturerId equals manufacturers.Id into manufacturersGroup
                              from manufacturers in manufacturersGroup.DefaultIfEmpty()
                              join model in context.Models on equipment.ModelId equals model.Id into modelGroup
                              from model in modelGroup.DefaultIfEmpty()
                              where equipment.MaintenanceEntitiesId == maintenanceId
                              select new EquipmentDto()
                              {
                                  Id = equipment.Id,
                                  Serial_number = equipment.SerialNumber,
                                  Inventory_number = equipment.InventoryNumber,
                                  Kind = kind != null ? new()
                                  {
                                      Id = kind.Id,
                                      Name = kind.Name,
                                      Code = kind.Code
                                  } : null,
                                  Manufacturer = manufacturers != null ? new()
                                  {
                                      Id = manufacturers.Id,
                                      Name = manufacturers.Name,
                                      Code = manufacturers.Code
                                  } : null,
                                  Model = model != null ? new()
                                  {
                                      Id = model.Id,
                                      Name = model.Name,
                                      Code = model.Code
                                  } : null
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment list by maintenance entity.");
                return null;
            }
        }

        public async Task<IEnumerable<EquipmentDto>?> GetEquipmentsByCompany(int companyId)
        {
            try
            {
                return await (from equipment in context.Equipment
                              join kind in context.Kinds on equipment.KindId equals kind.Id into kindGroup
                              from kind in kindGroup.DefaultIfEmpty()
                              join manufacturers in context.Manufacturers on equipment.ManufacturerId equals manufacturers.Id into manufacturersGroup
                              from manufacturers in manufacturersGroup.DefaultIfEmpty()
                              join model in context.Models on equipment.ModelId equals model.Id into modelGroup
                              from model in modelGroup.DefaultIfEmpty()
                              where equipment.CompanyId == companyId
                              select new EquipmentDto()
                              {
                                  Id = equipment.Id,
                                  Serial_number = equipment.SerialNumber,
                                  Inventory_number = equipment.InventoryNumber,
                                  Kind = kind != null ? new()
                                  {
                                      Id = kind.Id,
                                      Name = kind.Name,
                                      Code = kind.Code
                                  } : null,
                                  Manufacturer = manufacturers != null ? new()
                                  {
                                      Id = manufacturers.Id,
                                      Name = manufacturers.Name,
                                      Code = manufacturers.Code
                                  } : null,
                                  Model = model != null ? new()
                                  {
                                      Id = model.Id,
                                      Name = model.Name,
                                      Code = model.Code
                                  } : null
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment list by company.");
                return null;
            }
        }

        public async Task<EquipmentDto?> GetEquipmentById(int equipmentId)
        {
            try
            {
                return await (from equipment in context.Equipment
                              join kind in context.Kinds on equipment.KindId equals kind.Id into kindGroup
                              from kind in kindGroup.DefaultIfEmpty()
                              join manufacturers in context.Manufacturers on equipment.ManufacturerId equals manufacturers.Id into manufacturersGroup
                              from manufacturers in manufacturersGroup.DefaultIfEmpty()
                              join model in context.Models on equipment.ModelId equals model.Id into modelGroup
                              from model in modelGroup.DefaultIfEmpty()
                              where equipment.Id == equipmentId
                              select new EquipmentDto()
                              {
                                  Id = equipment.Id,
                                  Serial_number = equipment.SerialNumber,
                                  Inventory_number = equipment.InventoryNumber,
                                  Kind = kind != null ? new()
                                  {
                                      Id = kind.Id,
                                      Name = kind.Name,
                                      Code = kind.Code
                                  } : null,
                                  Manufacturer = manufacturers != null ? new()
                                  {
                                      Id = manufacturers.Id,
                                      Name = manufacturers.Name,
                                      Code = manufacturers.Code
                                  } : null,
                                  Model = model != null ? new()
                                  {
                                      Id = model.Id,
                                      Name = model.Name,
                                      Code = model.Code
                                  } : null
                              }).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by id.");
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
                _logger.LogError(ex, "Error retrieving equipment.");
                return null;
            }
        }

        public void Update(Equipment item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(Equipment item)
        {
            context.Equipment.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Equipment> items)
        {
            foreach (var item in items)
            {
                if (await GetItem(item, false) == null)
                    Create(item);
                else
                    Update(item);
            }
        }
    }
}
