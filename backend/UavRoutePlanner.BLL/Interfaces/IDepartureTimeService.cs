using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IDepartureTimeService
{

    Task<ErrorOr<DepartureRecommendationDto>> FindBest(
        long userId, long missionId, DepartureSearchDto request, CancellationToken ct = default);

    Task<ErrorOr<MissionDto>> SavePlan(
        long userId, long missionId, SaveDeparturePlanDto request, CancellationToken ct = default);
}
