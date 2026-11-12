using System.Globalization;
using System.Text;
using System.Text.Json;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Services;

public class CsvFlightTaskFormatter : IFlightTaskFormatter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public ExportFormat Format => ExportFormat.Csv;

    public ExportedFileDto Create(MissionDto mission, RouteResultDto route) => new(
        $"route_{route.MissionId}_{route.Id}.csv", "text/csv; charset=utf-8", WithBom(BuildCsv(route)));

    private static string BuildCsv(RouteResultDto route)
    {
        var sb = new StringBuilder();
        sb.Append("Order,Point,Latitude,Longitude,DistanceM,TrackDeg,HeadingDeg,GroundSpeedMs,TimeS\r\n");

        foreach (var leg in route.Legs)
        {
            sb.Append(leg.Order.ToString(Inv)).Append(',')
                .Append(CsvField(leg.PointName)).Append(',')
                .Append(leg.Latitude.ToString("F6", Inv)).Append(',')
                .Append(leg.Longitude.ToString("F6", Inv)).Append(',')
                .Append(leg.DistanceM.ToString("F1", Inv)).Append(',')
                .Append(leg.TrackDeg.ToString("F1", Inv)).Append(',')
                .Append(leg.HeadingDeg.ToString("F1", Inv)).Append(',')
                .Append(leg.GroundSpeedMs.ToString("F2", Inv)).Append(',')
                .Append(leg.TimeS.ToString("F1", Inv)).Append("\r\n");
        }

        sb.Append("Total,,,,")
            .Append(route.Legs.Sum(l => l.DistanceM).ToString("F1", Inv)).Append(",,,,")
            .Append(route.TotalTimeS.ToString("F1", Inv)).Append("\r\n");

        return sb.ToString();
    }

    private static string CsvField(string value)
    {
        // Захист від виконання формул, якщо файл відкриють у табличному редакторі, плюс стандартне екранування.
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@')
            value = "'" + value;

        return value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static byte[] WithBom(string text) =>
        Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray();
}
