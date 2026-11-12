using System.Security.Claims;
using UavRoutePlanner.Api.Contracts.Request.Route;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.Api.Endpoints;

public static class RouteEndpoint
{
    public static void MapRouteEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/missions/{missionId:long}/routes")
            .WithTags("Routes")
            .RequireAuthorization();

        group.MapPost("", Build);
        group.MapGet("", GetAll);

        group.MapPost("compare", Compare);
        group.MapGet("{resultId:long}", GetById);

      
        group.MapGet("{resultId:long}/export", Export);

        group.MapPost("best-departure", FindBestDeparture);
    }

    private static async Task<IResult> FindBestDeparture(
        long missionId, DepartureSearchRequest? request, ClaimsPrincipal user,
        IDepartureTimeService service, CancellationToken ct) =>
        (await service.FindBest(user.GetUserId(), missionId, (request ?? new DepartureSearchRequest(null, null)).ToDto(), ct))
        .ToResult();

    private static async Task<IResult> Build(
        long missionId, RouteRequest? request, ClaimsPrincipal user, IRouteBuildingService service, CancellationToken ct) =>
        (await service.Build(user.GetUserId(), missionId, (request ?? new RouteRequest(default, null, null, null)).ToDto(), ct))
        .ToResult();

    private static async Task<IResult> Compare(
        long missionId, RouteRequest? request, ClaimsPrincipal user, IRouteComparisonService service, CancellationToken ct) =>
        (await service.Compare(user.GetUserId(), missionId, (request ?? new RouteRequest(default, null, null, null)).ToDto(), ct))
        .ToResult();

    private static async Task<IResult> Export(
        long missionId, long resultId, string? format, ClaimsPrincipal user, IRouteExportService service, CancellationToken ct)
    {
        if (!Enum.TryParse<ExportFormat>(format ?? nameof(ExportFormat.Csv), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: Errors.ExportFormatUnknown);

        return (await service.Export(user.GetUserId(), missionId, resultId, parsed, ct)).ToFileResult();
    }

    private static async Task<IResult> GetAll(
        long missionId, ClaimsPrincipal user, IRouteQueryService service, CancellationToken ct) =>
        (await service.GetByMission(user.GetUserId(), missionId, ct)).ToResult();

    private static async Task<IResult> GetById(
        long missionId, long resultId, ClaimsPrincipal user, IRouteQueryService service, CancellationToken ct) =>
        (await service.GetById(user.GetUserId(), missionId, resultId, ct)).ToResult();
}
