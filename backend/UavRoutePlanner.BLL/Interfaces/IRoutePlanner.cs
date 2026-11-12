using ErrorOr;
using UavRoutePlanner.BLL.Models.Flight;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IRoutePlanner
{
  
    
    ErrorOr<RoutePlan> Plan(RoutePlanRequest request, CancellationToken ct = default);
}
