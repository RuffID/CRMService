using CRMService.Core;
using CRMService.DataBase;
using CRMService.Service.DataBase;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"))
    .AddJsonFile("appsettings.json");

Log.Logger = new LoggerConfiguration()
    .Enrich.With(new SimpleClassNameEnricher())
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

Log.Information("[Class:{ClassName}] Starting CRM service.", nameof(Program));

try
{
    builder.Host.UseSerilog();
    builder.Services.AddConfig(builder.Configuration);
    builder.Services.ConfigureServices(builder.Configuration);   

    WebApplication app = builder.Build();

    Log.Information("[Class:{ClassName}] CRM service started.", nameof(Program));

    using (IServiceScope scope = app.Services.CreateScope())
    {
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        IHostApplicationLifetime lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

        try
        {
            CRMEntitiesContext db = scope.ServiceProvider.GetRequiredService<CRMEntitiesContext>();
            MigrationService<CRMEntitiesContext> migrationService = scope.ServiceProvider.GetRequiredService<MigrationService<CRMEntitiesContext>>();
            await migrationService.MigrateDatabaseWithBackup();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "[Class:{ClassName}] Migration failed. Stopping application.", nameof(Program));
            lifetime.StopApplication();
            return;
        }

    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownProxies =
        {
            IPAddress.Parse("127.0.0.1"), // запуск вне docker
            IPAddress.Parse("172.18.0.1") // если nginx с точки зрения контейнера
        },
    });

    if (!app.Environment.IsDevelopment())    
        app.UseExceptionHandler("/Error");    

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();
    app.MapControllers();    

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "[Class:{ClassName}] Internal application error.", nameof(Program));
}
finally
{
    Log.Information("[Class:{ClassName}] CRM service stopped.", nameof(Program));
    await Log.CloseAndFlushAsync();
}