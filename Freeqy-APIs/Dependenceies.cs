using System.Reflection;
using Mapster;
using MapsterMapper;

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
}