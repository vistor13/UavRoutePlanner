using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using UavRoutePlanner.Api.Endpoints;
using UavRoutePlanner.BLL;

namespace UavRoutePlanner.Api.Extensions;

public static class WebApplicationExtensions
{
    private const string CorsPolicy = "AllowAngular";

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

       
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGenServicePortfolio();
        services.AddBLLLayer(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicy, policy => policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition"));
        });
    }

    public static void UseApplicationMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0);
            app.UseSwaggerUI();
        }

        app.UseCors(CorsPolicy);
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthEndpoint();
        app.MapUavProfileEndpoint();
        app.MapMissionEndpoint();
        app.MapWeatherEndpoint();
        app.MapRouteEndpoint();
        app.MapObjectImportEndpoint();
    }
}