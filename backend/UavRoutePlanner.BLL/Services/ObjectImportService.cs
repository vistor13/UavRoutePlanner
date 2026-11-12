using System.Text.Json;
using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Import;
using Msg = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class ObjectImportService(IEnumerable<IObjectFileParser> parsers) : IObjectImportService
{
    private const int MaxContentLength = 1_000_000;

    private readonly IReadOnlyList<IObjectFileParser> _parsers = parsers.ToList();

    public ErrorOr<ObjectImportResultDto> Parse(ObjectImportRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return Error.Validation("ImportEmpty", Msg.ImportEmpty);

        if (request.Content.Length > MaxContentLength)
            return Error.Validation("ImportTooLarge", Msg.ImportTooLarge);

        var parser = _parsers.FirstOrDefault(p => p.CanParse(request.FileName, request.Content));
        if (parser is null)
            return Error.Validation("ImportFormatUnsupported", Msg.ImportFormatUnsupported);

        var collector = new ImportCollector();

        try
        {
            if (parser.Parse(request.Content, collector) is { } error)
                return error;
        }
        catch (JsonException)
        {
            return Error.Validation("ImportInvalidJson", Msg.ImportInvalidJson);
        }

        if (collector.Objects.Count == 0 && collector.Errors.Count == 0)
            collector.AddError(0, Msg.ImportNoObjects);

        return new ObjectImportResultDto(collector.Objects, collector.Errors);
    }
}
