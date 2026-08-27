using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.CrmEntity;

namespace CRMService.Infrastructure.DataBase.Repository
{
    public class PlanSettingsUnitOfWork(
        IUnitOfWorkScope scope,
        IPlanRepository plan,
        IGeneralSettingsRepository generalSettings,
        IPlanSettingRepository planSetting,
        IPlanColorSchemeRepository planColor) : MainScenarioUnitOfWork(scope), IPlanSettingsUnitOfWork
    {
        public IPlanRepository Plan { get; } = plan;
        public IGeneralSettingsRepository GeneralSettings { get; } = generalSettings;
        public IPlanSettingRepository PlanSetting { get; } = planSetting;
        public IPlanColorSchemeRepository PlanColor { get; } = planColor;
    }
}
