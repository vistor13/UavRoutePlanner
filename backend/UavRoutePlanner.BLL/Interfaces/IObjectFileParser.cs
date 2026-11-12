using UavRoutePlanner.BLL.Models.Dto.Import;

namespace UavRoutePlanner.BLL.Interfaces;


public interface IObjectFileParser
{
    
    bool CanParse(string? fileName, string content);

   
    ErrorOr.Error? Parse(string content, IImportSink sink);
}


public interface IImportSink
{
    void Add(int line, string? name, double latitude, double longitude);

    void AddError(int line, string message);
}
