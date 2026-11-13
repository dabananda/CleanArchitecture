using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Infrastructure.Configurations;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Repositories;
using CleanArchitecture.Infrastructure.Security;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Settings settings)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(settings.ConnectionStrings.SqlServer);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, DistributedCacheService>();

        services.AddSingleton(settings);
        services.AddSingleton(settings.Cloudinary);

        services.AddScoped<IImageService, CloudinaryImageService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
