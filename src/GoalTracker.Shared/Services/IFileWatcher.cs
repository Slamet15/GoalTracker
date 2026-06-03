namespace GoalTracker.Shared.Services;

public interface IFileWatcher
{
    event EventHandler? DataFileChanged;
    void Start();
}
