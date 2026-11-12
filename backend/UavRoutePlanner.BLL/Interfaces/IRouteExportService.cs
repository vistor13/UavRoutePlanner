using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IRouteExportService
{
    
    Task<ErrorOr<ExportedFileDto>> Export(long userId, long missionId, long resultId, ExportFormat format, CancellationToken ct = default);
}
