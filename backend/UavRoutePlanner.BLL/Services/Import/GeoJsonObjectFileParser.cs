using System.Text.Json;
using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using Msg = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class GeoJsonObjectFileParser : IObjectFileParser
{
    public bool CanParse(string? fileName, string content) =>
        Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
        {
            ".geojson" or ".json" => true,
            ".csv" or ".txt" => false,
            _ => content.TrimStart().StartsWith('{')
        };

    public Error? Parse(string content, IImportSink sink)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var index = 0;

        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("type", out var typeElement))
        {
            sink.AddError(0, Msg.ImportNotGeoJson);
            return null;
        }

        switch (typeElement.GetString())
        {
            case "FeatureCollection":
                if (root.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
                    foreach (var feature in features.EnumerateArray())
                        ReadFeature(feature, ++index, sink);
                break;
            case "Feature":
                ReadFeature(root, 1, sink);
                break;
            default:
                ReadGeometry(root, null, 1, sink);
                break;
        }

        return null;
    }

    private static void ReadFeature(JsonElement feature, int index, IImportSink sink)
    {
        if (feature.ValueKind != JsonValueKind.Object
            || !feature.TryGetProperty("geometry", out var geometry)
            || geometry.ValueKind != JsonValueKind.Object)
        {
            sink.AddError(index, Msg.ImportNoGeometry);
            return;
        }

        ReadGeometry(geometry, ReadName(feature), index, sink);
    }

    private static string? ReadName(JsonElement feature)
    {
        if (!feature.TryGetProperty("properties", out var properties) || properties.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var key in new[] { "name", "Name", "title", "label" })
        {
            if (properties.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
        }

        return null;
    }

    private static void ReadGeometry(JsonElement geometry, string? name, int index, IImportSink sink)
    {
        var type = geometry.TryGetProperty("type", out var t) ? t.GetString() : null;

        if (!geometry.TryGetProperty("coordinates", out var coordinates))
        {
            sink.AddError(index, Msg.ImportNoGeometry);
            return;
        }

        switch (type)
        {
            case "Point":
                ReadPosition(coordinates, name, index, sink);
                break;
            case "MultiPoint" when coordinates.ValueKind == JsonValueKind.Array:
                foreach (var position in coordinates.EnumerateArray())
                    ReadPosition(position, name, index, sink);
                break;
            default:
                sink.AddError(index, string.Format(Msg.ImportGeometryUnsupported, type ?? "?"));
                break;
        }
    }

    private static void ReadPosition(JsonElement position, string? name, int index, IImportSink sink)
    {
        if (position.ValueKind != JsonValueKind.Array || position.GetArrayLength() < 2
            || position[0].ValueKind != JsonValueKind.Number || position[1].ValueKind != JsonValueKind.Number)
        {
            sink.AddError(index, Msg.ImportNotANumber);
            return;
        }

        // GeoJSON: [довгота, широта].
        sink.Add(index, name, position[1].GetDouble(), position[0].GetDouble());
    }
}
