using System.Text.Json;
using GoalTracker.Shared.Helpers;
using GoalTracker.Shared.Models;

namespace GoalTracker.Shared.Services;

public class SettingsService
{
    public AppSettings Load()
    {
        if (!File.Exists(DataPaths.SettingsFile)) return new AppSettings();
        try
        {
            var json = File.ReadAllText(DataPaths.SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json, DataService.JsonOptions)
                   ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        DataPaths.EnsureDirectoryExists();
        File.WriteAllText(DataPaths.SettingsFile,
            JsonSerializer.Serialize(settings, DataService.JsonOptions));
    }
}
