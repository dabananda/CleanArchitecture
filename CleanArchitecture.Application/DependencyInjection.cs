using CleanArchitecture.Application.Behaviors;
using CleanArchitecture.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CleanArchitecture.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<ISender, Sender>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var implementations = assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .ToArray();

        RegisterOpenGeneric(
            services,
            implementations,
            typeof(IRequestHandler<,>));

        RegisterOpenGeneric(
            services,
            implementations,
            typeof(IPipelineBehavior<,>));

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    private static void RegisterOpenGeneric(IServiceCollection services, IEnumerable<Type> implementations, Type openGenericType)
    {
        foreach (var implementation in implementations)
        {
            foreach (var service in implementation.GetInterfaces())
            {
                if (!service.IsGenericType)
                    continue;

                if (service.GetGenericTypeDefinition() != openGenericType)
                    continue;

                if (implementation.IsGenericTypeDefinition)
                {
                    services.AddScoped(service.GetGenericTypeDefinition(), implementation);
                }
                else
                {
                    services.AddScoped(service, implementation);
                }
            }
        }
    }
}
