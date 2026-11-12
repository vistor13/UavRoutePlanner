using UavRoutePlanner.BLL.Models.Dto.Import;

namespace UavRoutePlanner.Api.Contracts.Request;


public record ObjectImportRequest(string? FileName, string? Content)
{
    public ObjectImportRequestDto ToDto() => new(FileName, Content);
}
