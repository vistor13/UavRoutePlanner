namespace UavRoutePlanner.BLL.Models.Dto.Route;

public enum ExportFormat
{
    Csv = 0,

    GeoJson = 1,

    Waypoints = 2
}

public record ExportedFileDto(string FileName, string ContentType, byte[] Content);
