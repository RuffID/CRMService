using CRMService.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Читает из appsettings.json
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting CRM service.");

try
{
    builder.Services.AddConfig(builder.Configuration);
    builder.Services.ConfigureServices(builder.Configuration);   

    var app = builder.Build();

    using (IServiceScope scope = app.Services.CreateScope())
    {
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("CRM service started.");
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownProxies = { IPAddress.Parse("127.0.0.1") }, // IP nginx (если на той же машине)
    });

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();    

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Internal application error.");
}
finally
{
    Log.Information("CRM service stopped.");
    await Log.CloseAndFlushAsync();
}