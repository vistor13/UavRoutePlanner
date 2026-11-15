namespace UavRoutePlanner.BLL.Interfaces;

public interface ILoginAttemptTracker
{
    bool IsLocked(string userName);

    void RegisterFailure(string userName);

    void Reset(string userName);
}
