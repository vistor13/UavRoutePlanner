using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Services;

/// <summary>Обирає формувач за форматом із зареєстрованих. Сам формати не знає.</summary>
public class FlightTaskExporter(IEnumerable<IFlightTaskFormatter> formatters) : IFlightTaskExporter
{
    private readonly Dictionary<ExportFormat, IFlightTaskFormatter> _formatters =
        formatters.ToDictionary(f => f.Format);

    public ExportedFileDto Export(MissionDto mission, RouteResultDto route, ExportFormat format) =>
        _formatters.TryGetValue(format, out var formatter)
            ? formatter.Create(mission, route)
            : throw new ArgumentOutOfRangeException(nameof(format), format, null);
}
