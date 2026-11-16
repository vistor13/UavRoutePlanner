using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UavRoutePlanner.DAL.DataBase;
using UavRoutePlanner.DAL.DataBase.Repository;
using UavRoutePlanner.DAL.Interfaces;

namespace UavRoutePlanner.DAL;

public static class DependencyInjectionExtension
{

    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUavProfileRepository, UavProfileRepository>();
        services.AddScoped<IUavProfilePhotoRepository, UavProfilePhotoRepository>();
        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<IRouteResultRepository, RouteResultRepository>();
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

        return services;
    }
}
