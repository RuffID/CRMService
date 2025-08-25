using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Models.Dto.Entity;

namespace CRMService.Repository.Entity
{
    public class ParameterRepository(ApplicationContext context, ILoggerFactory logger) : IParameterRepository
    {
        private readonly ILogger<ParameterRepository> _logger = logger.CreateLogger<ParameterRepository>();

        public async Task<IEnumerable<Parameter>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Parameters.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving parameter list.", nameof(GetItems));
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving parameter.", nameof(GetItem));
                return null;
            }
        }

        public async Task<IEnumerable<EquipmentParameterDto>?> GetParameterByEquipmentId(int equipmentId)
        {
            try
            {
                return await context.Parameters
                    .AsNoTracking()
                    .Where(p => p.EquipmentId == equipmentId && p.KindParameter != null)
                    .Select(p => new EquipmentParameterDto
                    {
                        Id = p.Id,
                        Name = p.KindParameter.Name,
                        Code = p.KindParameter.Code,
                        Value = Convert.ToString(p.Value)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving parameter by equipment id.", nameof(GetParameterByEquipmentId));
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
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving parameter by equipment and kind parameter id.", nameof(GetParameterByEquipmentAndKindParameterId));
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
