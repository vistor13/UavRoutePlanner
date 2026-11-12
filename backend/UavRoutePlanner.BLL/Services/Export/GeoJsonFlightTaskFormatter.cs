using System.Globalization;
using System.Text;
using System.Text.Json;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Services;

public class GeoJsonFlightTaskFormatter : IFlightTaskFormatter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public ExportFormat Format => ExportFormat.GeoJson;

    public ExportedFileDto Create(MissionDto mission, RouteResultDto route) => new(
        $"route_{route.MissionId}_{route.Id}.geojson", "application/geo+json", BuildGeoJson(mission, route));

    private static byte[] BuildGeoJson(MissionDto mission, RouteResultDto route)
    {
        using var stream = new MemoryStream();
        using (var w = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            w.WriteStartObject();
            w.WriteString("type", "FeatureCollection");
            w.WriteStartArray("features");

      
            w.WriteStartObject();
            w.WriteString("type", "Feature");
            w.WriteStartObject("properties");
            w.WriteString("kind", "route");
            w.WriteString("windMode", route.WindMode.ToString());
            w.WriteNumber("windSpeedMs", route.WindSpeedMs);
            w.WriteNumber("windDirectionFromDeg", route.WindDirectionFromDeg);
            w.WriteString("departureTimeUtc", route.DepartureTimeUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", Inv));
            w.WriteNumber("visitedObjects", route.VisitedObjectsCount);
            w.WriteNumber("totalObjects", route.TotalObjectsCount);
            w.WriteNumber("totalTimeS", Math.Round(route.TotalTimeS, 1));
            w.WriteEndObject();
            w.WriteStartObject("geometry");
            w.WriteString("type", "LineString");
            w.WriteStartArray("coordinates");
            WriteCoordinate(w, mission.Start.Longitude, mission.Start.Latitude);
            foreach (var leg in route.Legs)
                WriteCoordinate(w, leg.Longitude, leg.Latitude);
            if (route.Legs.Count == 0)
                WriteCoordinate(w, mission.Finish.Longitude, mission.Finish.Latitude);
            w.WriteEndArray();
            w.WriteEndObject();
            w.WriteEndObject();


            WritePoint(w, "start", "Старт", mission.Start.Latitude, mission.Start.Longitude,
                order: 0, heading: route.Legs.Count > 0 ? route.Legs[0].HeadingDeg : null, skipped: false);

            for (var i = 0; i < route.Legs.Count; i++)
            {
                var leg = route.Legs[i];
                var isFinish = i == route.Legs.Count - 1;
                var heading = isFinish ? (double?)null : route.Legs[i + 1].HeadingDeg;

                WritePoint(w, isFinish ? "finish" : "object", leg.PointName, leg.Latitude, leg.Longitude,
                    leg.Order, heading, skipped: false, objectNumber: leg.ObjectNumber);
            }

            foreach (var skipped in route.SkippedObjects)
                WritePoint(w, "object", skipped.Name, skipped.Latitude, skipped.Longitude,
                    order: null, heading: null, skipped: true, objectNumber: skipped.Number);

            w.WriteEndArray();
            w.WriteEndObject();
        }

        return stream.ToArray();
    }

    private static void WriteCoordinate(Utf8JsonWriter w, double longitude, double latitude)
    {
        w.WriteStartArray();
        w.WriteNumberValue(Math.Round(longitude, 7));
        w.WriteNumberValue(Math.Round(latitude, 7));
        w.WriteEndArray();
    }

    private static void WritePoint(
        Utf8JsonWriter w, string kind, string name, double latitude, double longitude,
        int? order, double? heading, bool skipped, int? objectNumber = null)
    {
        w.WriteStartObject();
        w.WriteString("type", "Feature");
        w.WriteStartObject("properties");
        w.WriteString("kind", kind);
        w.WriteString("name", name);
        if (objectNumber is { } number) w.WriteNumber("objectNumber", number);
        if (order is { } o) w.WriteNumber("order", o);
        if (heading is { } h) w.WriteNumber("headingOutDeg", Math.Round(h, 1));
        w.WriteBoolean("skipped", skipped);
        w.WriteEndObject();
        w.WriteStartObject("geometry");
        w.WriteString("type", "Point");
        w.WritePropertyName("coordinates");
        WriteCoordinate(w, longitude, latitude);
        w.WriteEndObject();
        w.WriteEndObject();
    }
}
