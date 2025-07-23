using CRMService.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // „итает из appsettings.json
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("[Class:{ClassName}] Starting CRM service.", nameof(Program));

try
{
    builder.Services.AddConfig(builder.Configuration);
    builder.Services.ConfigureServices(builder.Configuration);   

    WebApplication app = builder.Build();

    using (IServiceScope scope = app.Services.CreateScope())
    {
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("[Class:{ClassName}] CRM service started.", nameof(Program));
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownProxies =
        {
            IPAddress.Parse("127.0.0.1"), // запуск вне docker
            IPAddress.Parse("172.18.0.1") // если nginx с точки зрени€ контейнера
        },
    });

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
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