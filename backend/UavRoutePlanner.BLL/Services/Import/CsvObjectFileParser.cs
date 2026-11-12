using System.Globalization;
using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using Msg = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;


public class CsvObjectFileParser : IObjectFileParser
{
    private static readonly string[] NameHeaders = ["name", "title", "id", "object", "назва", "об'єкт", "об’єкт", "обєкт"];
    private static readonly string[] LatHeaders = ["lat", "latitude", "y", "широта"];
    private static readonly string[] LonHeaders = ["lon", "lng", "long", "longitude", "x", "довгота"];

    public bool CanParse(string? fileName, string content) =>
        Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
        {
            ".csv" or ".txt" => true,
            ".geojson" or ".json" => false,
            _ => !content.TrimStart().StartsWith('{')
        };

    public Error? Parse(string content, IImportSink sink)
    {
        var lines = content.TrimStart('﻿').Split('\n');

        var rows = new List<(int Line, string Text)>();
        for (var i = 0; i < lines.Length; i++)
        {
            var text = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(text) || text.TrimStart().StartsWith('#')) continue;
            rows.Add((i + 1, text));
        }

        if (rows.Count == 0) return null;

        var delimiter = DetectDelimiter(rows[0].Text);
        var decimalComma = delimiter != ',';

        var first = SplitFields(rows[0].Text, delimiter);
        var hasHeader = first.All(f => !TryParseNumber(f, decimalComma, out _));

        int nameColumn = -1, latColumn, lonColumn;
        var start = 0;

        if (hasHeader)
        {
            var names = first.Select(f => f.Trim().ToLowerInvariant()).ToList();
            nameColumn = names.FindIndex(n => NameHeaders.Contains(n));
            latColumn = names.FindIndex(n => LatHeaders.Contains(n));
            lonColumn = names.FindIndex(n => LonHeaders.Contains(n));

            if (latColumn < 0 || lonColumn < 0)
                return Error.Validation("ImportHeaderUnknown", Msg.ImportHeaderUnknown);

            start = 1;
        }
        else
        {
          
            (nameColumn, latColumn, lonColumn) = first.Count >= 3 ? (0, 1, 2) : (-1, 0, 1);
        }

        var needed = Math.Max(Math.Max(nameColumn, latColumn), lonColumn) + 1;

        for (var r = start; r < rows.Count; r++)
        {
            var (line, text) = rows[r];
            var fields = SplitFields(text, delimiter);

            if (fields.Count < needed)
            {
                sink.AddError(line, Msg.ImportRowTooShort);
                continue;
            }

            if (!TryParseNumber(fields[latColumn], decimalComma, out var lat)
                || !TryParseNumber(fields[lonColumn], decimalComma, out var lon))
            {
                sink.AddError(line, Msg.ImportNotANumber);
                continue;
            }

            sink.Add(line, nameColumn >= 0 ? fields[nameColumn] : null, lat, lon);
        }

        return null;
    }

    private static char DetectDelimiter(string line) =>
        line.Contains(';') ? ';' : line.Contains('\t') ? '\t' : ',';

    private static bool TryParseNumber(string text, bool decimalComma, out double value)
    {
        text = text.Trim();
        if (decimalComma) text = text.Replace(',', '.');

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

  
    private static List<string> SplitFields(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (quoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else current.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == delimiter) { fields.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }

        fields.Add(current.ToString());
        return fields;
    }
}
