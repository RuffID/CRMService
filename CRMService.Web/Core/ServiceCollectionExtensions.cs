using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Abstractions.Database.Repository.CrmEntity;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Authorization;
using CRMService.Application.Service.CrmServices;
using CRMService.Application.Service.Hosted;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Report;
using CRMService.Application.Service.Sync;
using CRMService.Application.Service.Webhook;
using CRMService.Domain.Models.Constants;
using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.DataBase.Repository;
using CRMService.Infrastructure.DataBase.Repository.Authorization;
using CRMService.Infrastructure.DataBase.Repository.CrmEntity;
using CRMService.Infrastructure.DataBase.Repository.Entity;
using CRMService.Infrastructure.DataBase.Repository.OkdeskEntity;
using CRMService.Infrastructure.DataBase.Repository.Report;
using CRMService.Infrastructure.Service.Authorization;
using CRMService.Infrastructure.Service.DataBase;
using CRMService.Infrastructure.Service.Requests;
using CRMService.Web.Core.Filter;
using CRMService.Web.Core.Middleware;
using CRMService.Web.Models.Server;
using CRMService.Web.Service.BackgroundServices;
using EFCoreLibrary.Abstractions.Database;
using EFCoreLibrary.EfCore;
using EFCoreLibrary.Extensions;
using HttpClientLibrary;
using HttpClientLibrary.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CRMService.Web.Core
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration conf)
        {
            services.Configure<ApiEndpointOptions>(
                conf.GetSection(ApiEndpointOptions.SectionName));
            services.Configure<WebHookOkdeskOptions>(
                conf.GetSection(WebHookOkdeskOptions.SectionName));
            services.Configure<OkdeskOptions>(opt => { opt.OkdeskApiToken = conf[OkdeskOptions.SectionName]!; });
            services.Configure<TelegramBotOptions>(
                conf.GetSection(TelegramBotOptions.SectionName));
            services.Configure<AuthorizationOptions>(opt => { opt.JWTSymmetricSecurityKey = conf[AuthorizationOptions.SectionName]!; });

            return services;
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            AddConfig(services, builder.Configuration);
            AddRepositories(services);

            services.AddTransient<ExceptionHandlingMiddleware>();
            services.AddControllers();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Smart";
                options.DefaultAuthenticateScheme = "Smart";
                options.DefaultChallengeScheme = "Smart";
            })
            .AddPolicyScheme("Smart", "Smart auth", options =>
            {
                options.ForwardDefaultSelector = ctx =>
                {
                    var auth = ctx.Request.Headers[HeaderNames.Authorization].ToString();
                    if (!string.IsNullOrWhiteSpace(auth) &&
                        auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return JwtBearerDefaults.AuthenticationScheme;

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = ".CRMService.Cookies";
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/accessdenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);

                options.Events.OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                string key = builder.Configuration[AuthorizationOptions.SectionName]!;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JWTSettingsConstants.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = JWTSettingsConstants.AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuerSigningKey = true,
                };
                options.RequireHttpsMetadata = false;
            });

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

            // Определяет путь в зависимости от ОС для папки, где будут храниться ключи для Data Protection
            string keyPath;
            string projectName = Assembly.GetEntryAssembly()?.GetName().Name ?? "DefaultAppName";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                keyPath = Path.Combine(builder.Environment.ContentRootPath, "keys-windows");
            else
                keyPath = Path.Combine(builder.Environment.ContentRootPath, "keys-linux");

            // Убедиться, что папка существует
            Directory.CreateDirectory(keyPath);

            // Настроить Data Protection
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
                .SetApplicationName(projectName);

            services.AddDbContext<MainContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("MSSql")); });
            services.AddDbContext<OkdeskContext>(options => { options.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql")); });

            services.AddHttpClient<IHttpApiClient, HttpApiClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(180);
            });

            services.AddSignalR();

            services.AddScoped<IpOkdeskWebHookActionFilterAttribute>();
            services.AddScoped<IAppDbContext<MainContext>>(sp => new EfDbContextAdapter<MainContext>(sp.GetRequiredService<MainContext>()));
            services.AddScoped<IAppDbContext<OkdeskContext>>(sp => new EfDbContextAdapter<OkdeskContext>(sp.GetRequiredService<OkdeskContext>()));

            services.AddSingleton<EntitySyncService>();
            services.AddSingleton<ServerData>();
            services.AddScoped<DataBaseCheckUpService<MainContext>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(sp =>
            {
                ILogger<BackupService<MainContext>> logger = sp.GetRequiredService<ILogger<BackupService<MainContext>>>();
                string connectionString = builder.Configuration.GetConnectionString("MSSql")!;
                string backupFolder = OperatingSystem.IsLinux() ? "/var/opt/mssql/backups" : Path.Combine(AppContext.BaseDirectory, "Backups");
                return new BackupService<MainContext>(connectionString, backupFolder, logger);
            });

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
            services.AddScoped<Application.Service.OkdeskEntity.RoleService>();
            services.AddScoped<TimeEntryService>();

            services.AddScoped<IEmployeePerformanceReportService, EmployeePerformanceReportService>();
            services.AddScoped<ISpentTimeChartService, SpentTimeChartService>();
            services.AddScoped<IPlanSettingsService, PlanSettingsService>();

            services.AddScoped<IOkdeskEntityRequestService, GetOkdeskEntityService>();
            services.AddScoped<UpdateDirectoriesService>();
            services.AddScoped<Hasher>();
            services.AddScoped<IAccessTokenService, JwtTokenService>();
            services.AddScoped<IRandomStringGenerator, GenerateRandomString>();
            services.AddScoped<UserService>();
            services.AddScoped<Application.Service.Authorization.RoleService>();
            services.AddScoped<INotificationService, TelegramNotification>();

            services.AddScoped<IWebhookHandler, IssueWebhookService>();
            services.AddScoped<IWebhookHandler, CompanyWebhookService>();
            services.AddScoped<IWebhookHandler, MaintenanceEntityWebhookService>();
            services.AddScoped<IWebhookHandler, EquipmentWebhookService>();

#if !DEBUG
            services.AddHostedService<ThirtyMinutesReportHostedService>();
            services.AddHostedService<DailyReportHostedService>();
            services.AddHostedService<UpdateDirectoriesHostedService>();
#endif

            return services;
        }

        private static IServiceCollection AddRepositories(
             this IServiceCollection services)
        {
            services.AddEfCoreBaseRepositories<MainContext>();
            services.AddEfCoreBaseRepositories<OkdeskContext>();

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
            services.AddScoped<IIssueTypeGroupRepository, IssueTypeGroupRepository>();
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
            services.AddScoped<IOkdeskIssueRepository, OkdeskIssueRepository>();
            services.AddScoped<IOkdeskIssueStatusRepository, OkdeskIssueStatusRepository>();
            services.AddScoped<IOkdeskIssuePriorityRepository, OkdeskIssuePriorityRepository>();
            services.AddScoped<IOkdeskIssueTypeRepository, OkdeskIssueTypeRepository>();
            services.AddScoped<IOkdeskTimeEntryRepository, OkdeskTimeEntryRepository>();
            services.AddScoped<IOkdeskUnitOfWork, OkdeskUnitOfWork>();

            services.AddScoped<IEmployeePerformanceReportRepository, EmployeePerformanceReportRepository>();
            services.AddScoped<ISpentTimeChartReportRepository, SpentTimeChartReportRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IGeneralSettingsRepository, GeneralSettingsRepository>();
            services.AddScoped<IPlanSettingRepository, PlanSettingRepository>();
            services.AddScoped<IPlanColorSchemeRepository, PlanColorSchemeRepository>();

            return services;
        }
    }
}
