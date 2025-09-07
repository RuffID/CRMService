using CRMService.API;
using CRMService.Core.Filter;
using CRMService.DataBase;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Api;
using CRMService.Interfaces.Database;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Service;
using CRMService.Middleware;
using CRMService.Models.ConfigClass;
using CRMService.Models.Server;
using CRMService.Repository;
using CRMService.Repository.Authorization;
using CRMService.Service.Authorization;
using CRMService.Service.DataBase;
using CRMService.Service.Entity;
using CRMService.Service.Hosted;
using CRMService.Service.HostedServices;
using CRMService.Service.Report;
using CRMService.Service.Sync;
using CRMService.Service.Webhook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CRMService.Interfaces.Repository.Base;
using CRMService.Repository.Base;
using CRMService.Interfaces.Repository.Entity;
using CRMService.Repository.Entity;
using CRMService.Interfaces.Repository.Report;
using CRMService.Repository.Report;

namespace CRMService.Core
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddConfig(
             this IServiceCollection services, IConfiguration conf)
        {
            services.Configure<ApiEndpointOptions>(
                conf.GetSection(ApiEndpointOptions.SectionName));
            services.Configure<WebHookOkdeskOptions>(
                conf.GetSection(WebHookOkdeskOptions.SectionName));
            services.Configure<OkdeskOptions>(opt => { opt.OkdeskApiToken = conf["OkdeskApiToken"]!; });
            services.Configure<TelegramBotOptions>(
                conf.GetSection(TelegramBotOptions.SectionName));
            services.Configure<AuthorizationOptions>(opt => { opt.JWTSymmetricSecurityKey = conf["JWTSymmetricSecurityKey"]!; });

            return services;
        }

        public static IServiceCollection ConfigureServices(
             this IServiceCollection services, IConfiguration configuration)
        {
            AddConfig(services, configuration);
            AddRepositories(services);

            services.AddTransient<ExceptionHandlingMiddleware>();
            services.AddRazorPages(options =>
            {
                // Делает все ссылки на страницы to lower case
                options.Conventions.AddFolderRouteModelConvention("/", model =>
                {
                    foreach (SelectorModel selector in model.Selectors)
                    {
                        AttributeRouteModel? attrRoute = selector.AttributeRouteModel;
                        if (attrRoute?.Template != null)
                            attrRoute.Template = attrRoute.Template.ToLowerInvariant();
                    }
                });
            });

            services.AddDbContext<ApplicationContext>(options =>
            {
                string? connectionString = configuration.GetConnectionString("MSSql");
                options.UseSqlServer(connectionString);
            });

            services.AddControllers();
            services.AddLogging();
            services.AddAutoMapper(cfg => { }, typeof(MappingProfiles).Assembly);
            services.AddHttpClient<IRequestService, RequestClient>();
            services.AddSignalR();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<AuthorizationOptions>>((options, authOpt) =>
                {
                    AuthorizationOptions settings = authOpt.Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = JWTSettingsConstants.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = JWTSettingsConstants.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JWTSymmetricSecurityKey)),
                        ValidateIssuerSigningKey = true,
                    };

                    options.RequireHttpsMetadata = false;
                });

            services.AddScoped<IAppDbContext>(sp => new EfDbContextAdapter<ApplicationContext>(sp.GetRequiredService<ApplicationContext>()));
            services.AddSingleton(new PGConfig(configuration.GetConnectionString("Postgresql")!));
            services.AddSingleton<EntitySyncService>();
            services.AddSingleton<ServerData>();
            services.AddScoped<PGSelect>();
            services.AddScoped<IpOkdeskWebHookActionFilterAttribute>();

            services.AddScoped<IManageImage, ManageImage>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<GetItemService>();

            services.AddScoped<CompanyCategoryService>();
            services.AddScoped<CompanyService>();
            services.AddScoped<EmployeeService>();
            services.AddScoped<EquipmentService>();
            services.AddScoped<GroupService>();
            services.AddScoped<IssuePriorityService>();
            services.AddScoped<IssueService>();
            services.AddScoped<IssueStatusService>();
            services.AddScoped<IssueTypeService>();
            services.AddScoped<KindParameterService>();
            services.AddScoped<KindParamService>();
            services.AddScoped<KindService>();
            services.AddScoped<MaintenanceEntityService>();
            services.AddScoped<ManufacturerService>();
            services.AddScoped<ModelService>();
            services.AddScoped<RoleService>();
            services.AddScoped<TimeEntryService>();
            services.AddScoped<ReportService>();

            services.AddScoped<UpdateDirectoriesService>();

            services.AddScoped<GenerateRandomString>();
            services.AddScoped<JwtTokenService>();
            services.AddScoped<Hasher>();
            services.AddScoped<UserLoginService>();
            services.AddScoped<DataBaseCheckUpService<ApplicationContext>>();
            services.AddScoped<BackupService<ApplicationContext>>(sp =>
            {
                ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                string connectionString = configuration.GetConnectionString("MSSql")!;
                string backupFolder = OperatingSystem.IsLinux() ? "/var/opt/mssql/backups" : Path.Combine(AppContext.BaseDirectory, "Backups");
                return new BackupService<ApplicationContext>(connectionString, backupFolder, loggerFactory);
            });

            services.AddScoped<IWebhookHandler, IssueWebhookService>();
            services.AddScoped<IWebhookHandler, CompanyWebhookService>();
            services.AddScoped<IWebhookHandler, MaintenanceEntityWebhookService>();
            services.AddScoped<IWebhookHandler, EquipmentWebhookService>();

#if !DEBUG
            services.AddHostedService<ThirtyMinutesReportHostedService>();
            services.AddHostedService<UpdateDirectoriesHostedService>();
            services.AddHostedService<WeekReportHostedService>();
#endif

            return services;
        }

        private static IServiceCollection AddRepositories(
             this IServiceCollection services)
        {
            services.AddScoped(typeof(ICreateItemRepository<>), typeof(CreateItemRepository<>));
            services.AddScoped(typeof(IDeleteItemRepository<>), typeof(DeleteItemRepository<>));
            services.AddScoped(typeof(IGetItemByIdRepository<,>), typeof(GetItemByIdRepository<,>));
            services.AddScoped(typeof(IGetItemByPredicateRepository<>), typeof(GetItemByPredicateRepository<>));
            services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            services.AddScoped(typeof(IUpsertItemByIdRepository<,>), typeof(UpsertItemByIdRepository<,>));
            services.AddScoped(typeof(IUpsertItemByCodeRepository<>), typeof(UpsertItemByCodeRepository<>));
            services.AddScoped(typeof(IUpsertItemByPredicateRepository<>), typeof(UpsertItemByPredicateRepository<>));

            services.AddScoped<IBlockReasonRepository, BlockReasonRepository>();
            services.AddScoped<ICrmRoleRepository, CrmRoleRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanyCategoryRepository, CategoryRepository>();
            services.AddScoped<IEmployeeGroupRepository, EmployeeGroupRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeRoleRepository, EmployeeRoleRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IIssuePriorityRepository, IssuePriorityRepository>();
            services.AddScoped<IIssueTypeRepository, IssueTypeRepository>();
            services.AddScoped<IIssueStatusRepository, IssueStatusRepository>();
            services.AddScoped<IIssueRepository, IssueRepository>();
            services.AddScoped<IKindParameterRepository, KindParameterRepository>();
            services.AddScoped<IKindRepository, KindRepository>();
            services.AddScoped<IKindParamsRepository, KindParamsRepository>();
            services.AddScoped<IMaintenanceEntityRepository, MaintenanceEntityRepository>();
            services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
            services.AddScoped<IModelRepository, ModelRepository>();
            services.AddScoped<IOkdeskRoleRepository, OkdeskRoleRepository>();
            services.AddScoped<IParameterRepository, ParameterRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

            services.AddScoped<IReportRepository, ReportRepository>();

            return services;
        }
    }
}
