using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Import;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Services.Common;
using Msg = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;


public sealed class ImportCollector : IImportSink
{
    private const int MaxObjectsCount = 100;
    private const int MaxNameLength = 200;
    private const double CoincideThresholdM = 1.0;

    public List<ImportedObjectDto> Objects { get; } = [];
    public List<ImportErrorDto> Errors { get; } = [];
    private readonly List<int> _lines = [];
    private bool _limitReported;

    public void AddError(int line, string message) => Errors.Add(new ImportErrorDto(line, message));

    public void Add(int line, string? name, double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || !double.IsFinite(longitude)
            || latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            var swapped = Math.Abs(latitude) > 90 && Math.Abs(longitude) <= 90;
            Errors.Add(new ImportErrorDto(line, swapped ? Msg.ImportCoordinatesSwapped : Msg.ImportCoordinatesOutOfRange));
            return;
        }

        name = name?.Trim();
        if (name is { Length: > MaxNameLength })
        {
            Errors.Add(new ImportErrorDto(line, Msg.ImportNameTooLong));
            return;
        }

        if (Objects.Count >= MaxObjectsCount)
        {
            if (!_limitReported)
            {
                Errors.Add(new ImportErrorDto(line, Msg.ImportTooManyObjects));
                _limitReported = true;
            }

            return;
        }

        var point = new GeoPoint(latitude, longitude);
        for (var i = 0; i < Objects.Count; i++)
        {
            if (GeoMath.DistanceM(point, new GeoPoint(Objects[i].Latitude, Objects[i].Longitude)) < CoincideThresholdM)
            {
                Errors.Add(new ImportErrorDto(line, string.Format(Msg.ImportDuplicate, _lines[i])));
                return;
            }
        }

        Objects.Add(new ImportedObjectDto(
            string.IsNullOrEmpty(name) ? $"Об'єкт {Objects.Count + 1}" : name, latitude, longitude));
        _lines.Add(line);
    }
}
