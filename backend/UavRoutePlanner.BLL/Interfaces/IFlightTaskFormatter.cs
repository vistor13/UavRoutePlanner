using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

/// <summary>
/// Формувач польотного завдання в одному форматі. Новий формат додається окремим класом
/// і реєстрацією в DI, без змін в інших класах (принцип відкритості/закритості).
/// </summary>
public interface IFlightTaskFormatter
{
    ExportFormat Format { get; }

    ExportedFileDto Create(MissionDto mission, RouteResultDto route);
}
