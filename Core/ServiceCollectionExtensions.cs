using CRMService.API;
using CRMService.Core.Filter;
using CRMService.Core.Middleware;
using CRMService.DataBase;
using CRMService.DataBase.Postgresql;
using CRMService.DataBase.Repository;
using CRMService.DataBase.Repository.Authorization;
using CRMService.DataBase.Repository.Base;
using CRMService.DataBase.Repository.Entity;
using CRMService.DataBase.Repository.Report;
using CRMService.Interfaces.Api;
using CRMService.Interfaces.Database;
using CRMService.Interfaces.Repository;
using CRMService.Interfaces.Repository.Authorization;
using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.OkdeskEntity;
using CRMService.Interfaces.Repository.Report;
using CRMService.Interfaces.Service;
using CRMService.Models.ConfigClass;
using CRMService.Models.Constants;
using CRMService.Models.Server;
using CRMService.Service.Authorization;
using CRMService.Service.DataBase;
using CRMService.Service.Hosted;
using CRMService.Service.HostedServices;
using CRMService.Service.OkdeskEntity;
using CRMService.Service.Report;
using CRMService.Service.Sync;
using CRMService.Service.Webhook;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CRMService.Core
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

        public static IServiceCollection ConfigureServices(this IServiceCollection services, WebApplicationBuilder builder,
            Action<JsonSerializerSettings>? configureNewtonsoft = null, 
            Action<HttpClient>? configureHttpClient = null)
        {
            AddConfig(services, builder.Configuration);
            AddRepositories(services);

            services.AddTransient<ExceptionHandlingMiddleware>();
            services.AddControllers();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".CRMService.Cookies";
                options.LoginPath = "/Login";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".CRMService.Antiforgery";
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

            services.AddDbContext<ApplicationContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("MSSql"));});
            services.AddDbContext<OkdeskContext>(options => { options.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql"));});

            services.AddLogging();

            services.AddHttpClient<IHttpApiClient, HttpApiClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("br"));
                configureHttpClient?.Invoke(client);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpClientHandler handler = new ()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                };
                return handler;
            });

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

            services.AddScoped<IpOkdeskWebHookActionFilterAttribute>();
            services.AddScoped<IAppDbContext>(sp => new EfDbContextAdapter<ApplicationContext>(sp.GetRequiredService<ApplicationContext>()));
            services.AddSingleton(new PGConfig(builder.Configuration.GetConnectionString("Postgresql")!));
            services.AddSingleton<EntitySyncService>();
            services.AddSingleton<ServerData>();
            services.AddSingleton<IJsonSerializer>(sp =>
            {
                JsonSerializerSettings settings = new ();
                configureNewtonsoft?.Invoke(settings);
                return new NewtonsoftJsonSerializer(settings);
            });
            services.AddScoped<PGSelect>();
            services.AddScoped<DataBaseCheckUpService<ApplicationContext>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<BackupService<ApplicationContext>>(sp =>
            {
                ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                string connectionString = builder.Configuration.GetConnectionString("MSSql")!;
                string backupFolder = OperatingSystem.IsLinux() ? "/var/opt/mssql/backups" : Path.Combine(AppContext.BaseDirectory, "Backups");
                return new BackupService<ApplicationContext>(connectionString, backupFolder, loggerFactory);
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
            services.AddScoped<RoleService>();
            services.AddScoped<TimeEntryService>();
            services.AddScoped<ReportService>();

            services.AddScoped<GetOkdeskEntityService>();
            services.AddScoped<UpdateDirectoriesService>();
            services.AddScoped<Hasher>();

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

            services.AddScoped<IReportRepository, ReportRepository>();

            return services;
        }
    }
}