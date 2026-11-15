using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UavRoutePlanner.BLL.Algorithm;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Options;
using UavRoutePlanner.BLL.Services;
using UavRoutePlanner.DAL;

namespace UavRoutePlanner.BLL;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddBLLLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(o => o.SecretKey.Length >= AuthOptions.MinSecretKeyLength,
                $"Jwt:SecretKey має містити щонайменше {AuthOptions.MinSecretKeyLength} символів.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer не задано.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IWindTriangleCalculator, WindTriangleCalculator>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<ILoginAttemptTracker, LoginAttemptTracker>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUavProfileService, UavProfileService>();
        services.AddScoped<IMissionService, MissionService>();

        services.AddInfrastructureLayer(
            configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Не задано Database:ConnectionString."));

        services.AddSingleton<ITimeMatrixBuilder, TimeMatrixBuilder>();
        services.AddSingleton<IRoutePlanner, AntColonyRoutePlanner>();

        services.AddSingleton<IMissionValidator, MissionValidator>();

        services.AddOptions<OpenMeteoOptions>()
            .Bind(configuration.GetSection(OpenMeteoOptions.SectionName));
        services.AddHttpClient<IWeatherSource, OpenMeteoClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenMeteoOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.AddScoped<IHourlyWindProvider, HourlyWindProvider>();
        services.AddScoped<IWeatherForecastService, WeatherForecastService>();

        services.AddScoped<IWindResolver, WindResolver>();
        services.AddScoped<IRouteBuildingService, RouteBuildingService>();
        services.AddScoped<IRouteQueryService, RouteQueryService>();
        services.AddScoped<IRouteComparisonService, RouteComparisonService>();
        services.AddScoped<IRouteExportService, RouteExportService>();
        services.AddScoped<IDepartureTimeService, DepartureTimeService>();

        services.AddSingleton<IFlightTaskFormatter, CsvFlightTaskFormatter>();
        services.AddSingleton<IFlightTaskFormatter, GeoJsonFlightTaskFormatter>();
        services.AddSingleton<IFlightTaskFormatter, WaypointsFlightTaskFormatter>();
        services.AddSingleton<IFlightTaskExporter, FlightTaskExporter>();

        services.AddSingleton<IObjectFileParser, CsvObjectFileParser>();
        services.AddSingleton<IObjectFileParser, GeoJsonObjectFileParser>();
        services.AddSingleton<IObjectImportService, ObjectImportService>();

        return services;
    }
}
