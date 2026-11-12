using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Mission;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IMissionService
{
    Task<List<MissionSummaryDto>> GetAll(long userId, CancellationToken ct = default);

    Task<ErrorOr<MissionDto>> GetById(long userId, long id, CancellationToken ct = default);

    Task<ErrorOr<MissionDto>> Create(long userId, MissionInputDto input, CancellationToken ct = default);

    Task<ErrorOr<Deleted>> Delete(long userId, long id, CancellationToken ct = default);
}
