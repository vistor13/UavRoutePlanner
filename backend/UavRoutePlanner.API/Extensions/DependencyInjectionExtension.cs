using UavRoutePlanner.DAL.DataBase;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.Api.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
        {
            opt.UseNpgsql(configuration["Database:ConnectionString"],
                b => b.MigrationsAssembly("UavRoutePlanner.DAL"));
        });
        return services;
    }
}