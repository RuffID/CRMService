using CRMService.API;
using CRMService.Core.Filter;
using CRMService.DataBase;
using CRMService.DataBase.Postgresql;
using CRMService.HostedServices;
using CRMService.Interfaces.Api;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.ConfigClass;
using CRMService.Models.Server;
using CRMService.Repository;
using CRMService.Service.Authorization;
using CRMService.Service.Entity;
using CRMService.Service.Hosted;
using CRMService.Service.Report;
using CRMService.Service.Sync;
using CRMService.Service.Webhook;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CRMService.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfig(
             this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiEndpoint>(
                configuration.GetSection(ApiEndpoint.APIENDPOINTS));
            services.Configure<OkdeskSettings>(
                configuration.GetSection(OkdeskSettings.OKDESK));
            services.Configure<DatabaseSettings>(
                configuration.GetSection(DatabaseSettings.CONNECTION_STRINGS));
            services.Configure<TelegramBotSettings>(
                configuration.GetSection(TelegramBotSettings.TELEGRAM_BOT));
            services.Configure<AuthOptions>(
                configuration.GetSection(AuthOptions.AUTHORIZATION_OPTIONS));
            services.Configure<HashSettings>(
                configuration.GetSection(HashSettings.HASH_CONFIGURE));

            return services;
        }

        public static IServiceCollection ConfigureServices(
             this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorPages(options =>
            {
                // Делает все ссылки на страницы to lower case
                options.Conventions.AddFolderRouteModelConvention("/", model =>
                {
                    foreach (var selector in model.Selectors)
                    {
                        var attrRoute = selector.AttributeRouteModel;
                        if (attrRoute?.Template != null)
                        {
                            attrRoute.Template = attrRoute.Template.ToLowerInvariant();
                        }
                    }
                });
            });
            services.AddDbContext<CrmAuthorizationContext>();
            services.AddControllers();
            services.AddLogging();
            services.AddAutoMapper(cfg => { }, typeof(MappingProfiles).Assembly);
            services.AddHttpClient<IRequestService, RequestClient>();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["BearerToken:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["BearerToken:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["BearerToken:JWTKey"] ?? "")),
                        ValidateIssuerSigningKey = true,
                    };

                    options.RequireHttpsMetadata = false;
                });

            services.AddSingleton<EntitySyncService>();
            services.AddSingleton<ServerData>();
            services.AddScoped<PGSelect>();
            services.AddScoped<IpOkdeskWebHookActionFilterAttribute>();

            services.AddScoped<IManageImage, ManageImage>();
            services.AddScoped<IUnitOfWorkEntities, UnitOfWorkEntities>();
            services.AddScoped<IUnitOfWorkAuthorization, UnitOfWorkAuthorization>();


            // Services
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

            services.AddScoped<UserLoginService>();
            services.AddScoped<UserRegistrationService>();
            services.AddScoped<DataBaseHandler>();
            services.AddScoped<BackupService>();
            services.AddScoped<GenerateRandomString>();
            services.AddScoped<GenerateRefreshToken>();

            services.AddScoped<IWebhookHandler, IssueWebhookService>();
            services.AddScoped<IWebhookHandler, CompanyWebhookService>();
            services.AddScoped<IWebhookHandler, MaintenanceEntityWebhookService>();
            services.AddScoped<IWebhookHandler, EquipmentWebhookService>();


            services.AddHostedService<ThirtyMinutesReportHostedService>();
            services.AddHostedService<UpdateDirectoriesHostedService>();
            services.AddHostedService<WeekReportHostedService>();


            return services;
        }
    }
}
