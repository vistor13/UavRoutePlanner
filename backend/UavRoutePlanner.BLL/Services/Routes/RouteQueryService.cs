using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class RouteQueryService(IMissionRepository missionRepository) : IRouteQueryService
{
    public async Task<ErrorOr<List<RouteResultDto>>> GetByMission(
        long userId, long missionId, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        return mission.Results
            .OrderBy(r => r.DepartureTimeUtc)
            .ThenBy(r => r.CreatedAt)
            .Select(r => r.ToDto(mission))
            .ToList();
    }

    public async Task<ErrorOr<RouteResultDto>> GetById(
        long userId, long missionId, long resultId, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var result = mission.Results.FirstOrDefault(r => r.Id == resultId);

        return result is null
            ? Error.NotFound("RouteNotFound", Errors.RouteNotFound)
            : result.ToDto(mission);
    }
}
