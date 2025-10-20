using System.Reflection;
using Mapster;
using MapsterMapper;
using Freeqy_APIs.Repositories;
using Freeqy_APIs.Services;

namespace Freeqy_APIs;

public static class Dependenceies
{
    public static IServiceCollection AddDependency(this IServiceCollection services)
    {

        // Add services to the container.

        services.AddControllers();

        // Add swagger
        services.AddSwaggerGen();

        //Add Mapster
        services.AddMapsterDependcy();

        // Add Password Reset Services
        services.AddPasswordResetServices();
        
        return services;
    }

    private static IServiceCollection AddSwaggerDependency(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Freeqy Platform API",
                Version = "v1",
                Description = "Backend API for team formation and project management"
            });
        });
        return  services;
    }

    private static IServiceCollection AddMapsterDependcy(this IServiceCollection services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));
        return  services;
    }

    private static IServiceCollection AddPasswordResetServices(this IServiceCollection services)
    {
        // Register Password Reset Service
        services.AddScoped<IPasswordResetService, PasswordResetService>();

        // Register Email Service (Mock for now - replace with real implementation later)
        services.AddScoped<IEmailService, MockEmailService>();

        // Register Password Hasher (Mock for now - your teammate will replace with BCrypt/Argon2)
        services.AddScoped<IPasswordHasher, MockPasswordHasher>();

        // Register Mock Repositories as SINGLETON to maintain in-memory state across requests
        // When replaced with real EF Core implementations, change back to Scoped
        services.AddSingleton<IUserRepository, MockUserRepository>();
        services.AddSingleton<IPasswordResetTokenRepository, MockPasswordResetTokenRepository>();

        return services;
    }
}