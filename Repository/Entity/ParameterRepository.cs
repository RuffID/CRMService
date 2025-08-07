using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Dto.Entity;

namespace CRMService.Repository.Entity
{
    public class ParameterRepository(CrmEntitiesContext context, ILoggerFactory logger) : IParameterRepository
    {
        private readonly ILogger<ParameterRepository> _logger = logger.CreateLogger<ParameterRepository>();

        public async Task<IEnumerable<Parameter>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Parameters.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameter list.");
                return null;
            }
        }

        public async Task<Parameter?> GetItem(Parameter item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Parameters.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.Parameters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameter.");
                return null;
            }
        }

        public async Task<IEnumerable<EquipmentParameterDto>?> GetParameterByEquipmentId(int equipmentId)
        {
            try
            {
                return await (from parameter in context.Parameters
                              join kinds_parameters in context.KindsParameters on parameter.KindParameterId equals kinds_parameters.Id
                              where parameter.EquipmentId == equipmentId
                              select new EquipmentParameterDto()
                              {
                                  Id = parameter.Id,
                                  Name = kinds_parameters.Name,
                                  Code = kinds_parameters.Code,
                                  Value = Convert.ToString(parameter.Value)
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameter by equipment id.");
                return null;
            }
        }

        public async Task<Parameter?> GetParameterByEquipmentAndKindParameterId(Parameter parameter, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Parameters.FirstOrDefaultAsync(c => c.EquipmentId == parameter.EquipmentId && c.KindParameterId == parameter.KindParameterId);

                return await context.Parameters.AsNoTracking().FirstOrDefaultAsync(c => c.EquipmentId == parameter.EquipmentId && c.KindParameterId == parameter.KindParameterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parameter by equipment and kind parameter id.");
                return null;
            }
        }

        public void Update(Parameter oldItem, Parameter newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Parameter item)
        {
            context.Parameters.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Parameter> items)
        {
            foreach (var item in items.ToList())
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
