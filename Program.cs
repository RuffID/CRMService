using CRMService.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Читает из appsettings.json
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("[Class:{ClassName}] Starting CRM service.", nameof(Program));

try
{
    builder.Services.AddConfig(builder.Configuration);
    builder.Services.ConfigureServices(builder.Configuration);   

    var app = builder.Build();

    using (IServiceScope scope = app.Services.CreateScope())
    {
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("[Class:{ClassName}] CRM service started.", nameof(Program));
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
    Log.Fatal(ex, "[Class:{ClassName}] Internal application error.", nameof(Program));
}
finally
{
    Log.Information("[Class:{ClassName}] CRM service stopped.", nameof(Program));
    await Log.CloseAndFlushAsync();
}