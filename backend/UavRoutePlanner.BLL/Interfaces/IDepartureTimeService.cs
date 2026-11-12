using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IDepartureTimeService
{
    
    Task<ErrorOr<DepartureRecommendationDto>> FindBest(
        long userId, long missionId, DepartureSearchDto request, CancellationToken ct = default);
}
