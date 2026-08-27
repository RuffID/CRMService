using CRMService.Web.Core;
using CRMService.Application.DependencyInjection;
using CRMService.Infrastructure.DependencyInjection;
using CRMService.Web.Core.DependencyInjection;
using Serilog;
using CRMService.Web.Core.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using CRMService.Web.Core.Startup;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Testing"))
{
    string configPath = Path.Combine(AppContext.BaseDirectory, "Config", "config.json");
    builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: false);
}

Log.Logger = new LoggerConfiguration()
    .Enrich.With(new SimpleClassNameEnricher())
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWeb(builder);

WebApplication app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies =
        {
            IPAddress.Parse("127.0.0.1"),
            IPAddress.Parse("172.18.0.1"),
            IPAddress.Parse("192.168.1.16")
        },
});

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (IServiceScope scope = app.Services.CreateScope())
{
    IStartupInitializer initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
    await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();
