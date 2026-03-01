using CRMService.Web.Core;
using Serilog;
using CRMService.Infrastructure.Service.DataBase;
using CRMService.Web.Core.Middleware;
using CRMService.Infrastructure.DataBase;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string configPath = Path.Combine(AppContext.BaseDirectory, "Config", "config.json");

builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: false);

Log.Logger = new LoggerConfiguration()
    .Enrich.With(new SimpleClassNameEnricher())
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.ConfigureServices(builder);

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (IServiceScope scope = app.Services.CreateScope())
{
    DataBaseCheckUpService<ApplicationContext> dbCheckUp = scope.ServiceProvider.GetRequiredService<DataBaseCheckUpService<ApplicationContext>>();
    dbCheckUp.CheckOrUpdateDB();
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();



