using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Infrastructure.DataBase.Repository;
using CRMService.Infrastructure.DataBase.Repository.OkdeskEntity;
using CRMService.Infrastructure.DataBase;
using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.EfCore;
using EFCoreLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRMService.Infrastructure.IntegrationTests.Database;

internal static class OkdeskTestServices
{
    public static ServiceProvider Create(string connectionString)
    {
        ServiceCollection services = new();
        services.AddDbContext<OkdeskContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAppDbContext<OkdeskContext>>(provider =>
            new EfDbContextAdapter<OkdeskContext>(provider.GetRequiredService<OkdeskContext>()));
        services.AddEfCoreBaseRepositories<OkdeskContext>();

        services.AddScoped<IOkdeskCompanyDirectorySource, OkdeskCompanyDirectorySource>();
        services.AddScoped<IOkdeskEquipmentSource, OkdeskEquipmentSource>();
        services.AddScoped<IOkdeskCompanyCategoryRepository, OkdeskCompanyCategoryRepository>();
        services.AddScoped<IOkdeskCompanyRepository, OkdeskCompanyRepository>();
        services.AddScoped<IOkdeskEmployeeRepository, OkdeskEmployeeRepository>();
        services.AddScoped<IOkdeskGroupRepository, OkdeskGroupRepository>();
        services.AddScoped<IOkdeskKindRepository, OkdeskKindRepository>();
        services.AddScoped<IOkdeskKindParameterRepository, OkdeskKindParameterRepository>();
        services.AddScoped<IOkdeskKindParamsRepository, OkdeskKindParamsRepository>();
        services.AddScoped<IOkdeskMaintenanceEntityRepository, OkdeskMaintenanceEntityRepository>();
        services.AddScoped<IOkdeskManufacturerRepository, OkdeskManufacturerRepository>();
        services.AddScoped<IOkdeskModelRepository, OkdeskModelRepository>();
        services.AddScoped<IOkdeskEquipmentRepository, OkdeskEquipmentRepository>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
