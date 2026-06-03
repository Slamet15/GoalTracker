using GoalTracker.Shared.Models;

namespace GoalTracker.Shared.Services;

public interface IDataService
{
    Task<AppData> LoadAsync();
    Task SaveAsync(AppData data);
}
