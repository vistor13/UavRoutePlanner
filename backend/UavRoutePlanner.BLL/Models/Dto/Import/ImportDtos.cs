namespace UavRoutePlanner.BLL.Models.Dto.Import;

public record ObjectImportRequestDto(string? FileName, string? Content);

public record ImportedObjectDto(string Name, double Latitude, double Longitude);

public record ImportErrorDto(int Line, string Message);

public record ObjectImportResultDto(
    List<ImportedObjectDto> Objects,
    List<ImportErrorDto> Errors);
