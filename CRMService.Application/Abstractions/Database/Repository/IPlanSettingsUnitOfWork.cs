using CRMService.Application.Abstractions.Database.Repository.CrmEntity;

namespace CRMService.Application.Abstractions.Database.Repository
{
    public interface IPlanSettingsUnitOfWork : IUnitOfWorkScope
    {
        IPlanRepository Plan { get; }
        IGeneralSettingsRepository GeneralSettings { get; }
        IPlanSettingRepository PlanSetting { get; }
        IPlanColorSchemeRepository PlanColor { get; }
    }
}
