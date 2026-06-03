using GoalTracker.Shared.Helpers;

namespace GoalTracker.Shared.Services;

public class FileWatcherService : IFileWatcher, IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly SynchronizationContext? _syncContext;
    private DateTime _lastEvent = DateTime.MinValue;

    public event EventHandler? DataFileChanged;

    public FileWatcherService()
    {
        _syncContext = SynchronizationContext.Current;
    }

    public void Start()
    {
        DataPaths.EnsureDirectoryExists();

        _watcher = new FileSystemWatcher(
            Path.GetDirectoryName(DataPaths.AppDataFile)!,
            Path.GetFileName(DataPaths.AppDataFile))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastEvent).TotalMilliseconds < 300) return;
        _lastEvent = now;

        if (_syncContext is not null)
            _syncContext.Post(_ => DataFileChanged?.Invoke(this, EventArgs.Empty), null);
        else
            DataFileChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose() => _watcher?.Dispose();
}
