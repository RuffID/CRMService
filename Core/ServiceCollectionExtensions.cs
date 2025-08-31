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

namespace CRMService.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfig(
             this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiEndpointOptions>(
                configuration.GetSection(ApiEndpointOptions.APIENDPOINTS));
            services.Configure<OkdeskOptions>(
                configuration.GetSection(OkdeskOptions.OKDESK));
            services.Configure<TelegramBotOptions>(
                configuration.GetSection(TelegramBotOptions.TELEGRAM_BOT));
            services.Configure<AuthorizationOptions>(
                configuration.GetSection(AuthorizationOptions.AUTHORIZATION_OPTIONS));

            return services;
        }

        public static IServiceCollection ConfigureServices(
             this IServiceCollection services, IConfiguration configuration)
        {
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
                string? connectionString = configuration.GetConnectionString("MySqlServer");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.AddControllers();
            services.AddLogging();
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
            services.AddHttpClient<IRequestService, RequestClient>();
            services.AddSignalR();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<BearerTokenOptions>>((options, bearerOpt) =>
                {
                    BearerTokenOptions settings = bearerOpt.Value;

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

            services.AddScoped<IAppDbContext>(sp => new EfDbContextAdapter<ApplicationContext>(sp.GetRequiredService<ApplicationContext>()));

            services.AddScoped<BackupService<ApplicationContext>>(sp =>
            {
                ApplicationContext db = sp.GetRequiredService<ApplicationContext>();
                string backupDirectory = OperatingSystem.IsLinux() ? "/var/opt/mssql/backups" : Path.Combine(AppContext.BaseDirectory, "Backups");
                ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new BackupService<ApplicationContext>(db, backupDirectory, loggerFactory);
            });

            services.AddSingleton<EntitySyncService>();
            services.AddScoped<MigrationService<ApplicationContext>>();
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

            services.AddScoped<UserLoginService>();
            services.AddScoped<DataBaseHandler<ApplicationContext>>();
            services.AddScoped<BackupService<ApplicationContext>>();
            services.AddScoped<GenerateRandomString>();
            services.AddScoped<GenerateRefreshToken>();

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

        public static IServiceCollection AddRepositories(
             this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBlockReasonRepository, BlockReasonRepository>();
            services.AddScoped<ICrmRoleRepository, CrmRoleRepository>();

            return services;
        }
    }
}
