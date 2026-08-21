using Academy.Application.Behaviors;
using Academy.Application.Common.Localization;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Features.Notifications;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Academy.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddScoped<IRequestLanguage, RequestLanguage>();
        services.AddScoped<INotificationRealtimePublisher, NullNotificationRealtimePublisher>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
