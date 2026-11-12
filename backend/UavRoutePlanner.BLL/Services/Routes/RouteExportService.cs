using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class RouteExportService(
    IMissionRepository missionRepository,
    IFlightTaskExporter exporter) : IRouteExportService
{
    public async Task<ErrorOr<ExportedFileDto>> Export(
        long userId, long missionId, long resultId, ExportFormat format, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var result = mission.Results.FirstOrDefault(r => r.Id == resultId);
        if (result is null)
            return Error.NotFound("RouteNotFound", Errors.RouteNotFound);

        return exporter.Export(mission.ToDto(), result.ToDto(mission), format);
    }
}
