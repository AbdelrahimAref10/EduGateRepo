using System.Reflection;
using Academy.Application.Contracts.Notifications;
using Academy.Application.DependencyInjection;
using Academy.Infrastructure.DependencyInjection;
using Academy.Persistence.Seed;
using Academy.Server.Hubs;
using Academy.Server.Middlewares;
using Academy.Server.Notifications;
using Microsoft.AspNetCore.SignalR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 8 * 1024 * 1024;
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSingleton<IUserIdProvider, AcademyUserIdProvider>();
builder.Services.AddSignalR();
// Override Application null publisher with SignalR push.
builder.Services.AddSingleton<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Academy API";
    config.Version = "v1";
    config.AddSecurity("Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste your JWT access token."
    });
    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

var app = builder.Build();
var isOpenApiGeneration = IsOpenApiGeneration();

if (!isOpenApiGeneration)
{
    await IdentityDataSeeder.SeedAsync(app.Services);
}

app.UseExceptionHandler();

if (!isOpenApiGeneration)
{
    app.UseDefaultFiles();
    app.MapStaticAssets();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<RequestLanguageMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

if (!isOpenApiGeneration)
    app.MapFallbackToFile("/index.html");

app.Run();

static bool IsOpenApiGeneration()
{
    var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
    return entryAssemblyName is "NSwag.AspNetCore.Launcher" or "GetDocument.Insider";
}
