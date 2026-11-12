using UavRoutePlanner.Api.Contracts.Request;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;

namespace UavRoutePlanner.Api.Endpoints;

public static class ObjectImportEndpoint
{
    public static void MapObjectImportEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/objects")
            .WithTags("ObjectImport")
            .RequireAuthorization();

        // Розбір файлу CSV або GeoJSON з координатами об'єктів. Нічого не зберігає.
        group.MapPost("import", Import);
    }

    private static IResult Import(ObjectImportRequest request, IObjectImportService service) =>
        service.Parse(request.ToDto()).ToResult();
}
