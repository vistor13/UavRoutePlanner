using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Import;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IObjectImportService
{
    
    ErrorOr<ObjectImportResultDto> Parse(ObjectImportRequestDto request);
}
