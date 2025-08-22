using CRMService.API;
using CRMService.Core.Filter;
using CRMService.DataBase;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Api;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Service;
using CRMService.Models.ConfigClass;
using CRMService.Models.Server;
using CRMService.Repository;
using CRMService.Service.DataBase;
using CRMService.Service.Entity;
using CRMService.Service.Hosted;
using CRMService.Service.Report;
using CRMService.Service.Sync;
using CRMService.Service.Webhook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
            services.Configure<BearerToken>(
                configuration.GetSection(BearerToken.BEARER_TOKEN));

            return services;
        }

        public static IServiceCollection ConfigureServices(
             this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddLogging();
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
            services.AddHttpClient<IRequestService, RequestClient>();
            services.AddSignalR();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<BearerToken>>((options, bearerOpt) =>
                {
                    BearerToken settings = bearerOpt.Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = settings.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JWTKey ?? "")),
                        ValidateIssuerSigningKey = true,
                    };

                    options.RequireHttpsMetadata = false;
                });

            services.AddDbContext<CRMEntitiesContext>(options =>
            {
                string connectionSting = configuration["DatabaseSettings:MySqlMainCRM"] ?? "";
                options.UseMySql(connectionSting, ServerVersion.AutoDetect(connectionSting));
            });

            services.AddScoped<BackupService<CRMEntitiesContext>>(sp =>
            {
                CRMEntitiesContext db = sp.GetRequiredService<CRMEntitiesContext>();
                string backupDirectory = OperatingSystem.IsLinux() ? "/var/opt/mssql/backups" : Path.Combine(AppContext.BaseDirectory, "Backups");
                ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new BackupService<CRMEntitiesContext>(db, backupDirectory, loggerFactory);
            });

            services.AddSingleton<EntitySyncService>();
            services.AddScoped<MigrationService<CRMEntitiesContext>>();
            services.AddSingleton<ServerData>();
            services.AddScoped<PGSelect>();
            services.AddScoped<IpOkdeskWebHookActionFilterAttribute>();

            services.AddScoped<IManageImage, ManageImage>();
            services.AddScoped<IUnitOfWorkEntities, UnitOfWorkEntities>();
            services.AddScoped<IUnitOfWorkServerInfo, UnitOfWorkServerInfo>();

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
            services.AddScoped<IWebhookHandler, IssueWebhookService>();
            services.AddScoped<IWebhookHandler, CompanyWebhookService>();
            services.AddScoped<IWebhookHandler, MaintenanceEntityWebhookService>();
            services.AddScoped<IWebhookHandler, EquipmentWebhookService>();

            // Запуск служб только в RELEASE режиме
#if !DEBUG
            services.AddHostedService<ThirtyMinutesReportHostedService>();
            services.AddHostedService<UpdateDirectoriesHostedService>();
            services.AddHostedService<WeekReportHostedService>();
#endif

            return services;
        }
    }
}
