using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Mission;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IMissionValidator
{
   
    Error? Validate(MissionInputDto input);
}
