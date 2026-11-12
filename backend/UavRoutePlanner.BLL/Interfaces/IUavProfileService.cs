using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.UavProfile;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IUavProfileService
{
    Task<List<UavProfileDto>> GetAll(long userId, CancellationToken ct = default);

    Task<ErrorOr<UavProfileDto>> GetById(long userId, long id, CancellationToken ct = default);

    Task<ErrorOr<UavProfileDto>> Create(long userId, UavProfileInputDto input, CancellationToken ct = default);

    Task<ErrorOr<UavProfileDto>> Update(long userId, long id, UavProfileInputDto input, CancellationToken ct = default);

    Task<ErrorOr<Deleted>> Delete(long userId, long id, CancellationToken ct = default);
}
