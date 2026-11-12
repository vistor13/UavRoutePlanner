using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IFlightTaskExporter
{
    
    ExportedFileDto Export(MissionDto mission, RouteResultDto route, ExportFormat format);
}
