using System.Text.Json;
using System.Text.Json.Serialization;
using GoalTracker.Shared.Helpers;
using GoalTracker.Shared.Models;
using GoalTracker.Shared.Enums;

namespace GoalTracker.Shared.Services;

public class DataService : IDataService
{
    private const string MutexName = "GoalTracker_DataFile_Mutex";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<AppData> LoadAsync()
    {
        DataPaths.EnsureDirectoryExists();

        if (!File.Exists(DataPaths.AppDataFile))
            return new AppData();

        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                await using var fs = new FileStream(
                    DataPaths.AppDataFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                var data = await JsonSerializer.DeserializeAsync<AppData>(fs, JsonOptions)
                           ?? new AppData();

                EnrichComputedFields(data);
                return data;
            }
            catch (IOException) when (attempt < 4)
            {
                await Task.Delay(50 * (attempt + 1));
            }
        }

        return new AppData();
    }

    public async Task SaveAsync(AppData data)
    {
        DataPaths.EnsureDirectoryExists();
        data.LastModified = DateTime.UtcNow;

        using var mutex = new Mutex(false, MutexName);
        mutex.WaitOne(TimeSpan.FromSeconds(5));
        try
        {
            var tmpPath = DataPaths.AppDataFile + ".tmp";
            await using (var fs = new FileStream(
                tmpPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fs, data, JsonOptions);
                await fs.FlushAsync();
            }
            File.Move(tmpPath, DataPaths.AppDataFile, overwrite: true);
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static void EnrichComputedFields(AppData data)
    {
        // Build a fast category lookup
        var categoryMap = data.Categories.ToDictionary(c => c.Id);

        foreach (var goal in data.Goals)
        {
            // Progress: milestones take priority, then tasks, then manual
            if (goal.Milestones.Count > 0)
            {
                goal.ProgressPercent = goal.Milestones
                    .Where(m => m.IsCompleted)
                    .Sum(m => m.CompletionPercent);
                // Cap at 100 in case percentages were set over 100 total
                goal.ProgressPercent = Math.Min(100, goal.ProgressPercent);
            }
            else if (goal.UseTasksForProgress)
            {
                var linked = data.Tasks.Where(t => goal.LinkedTaskIds.Contains(t.Id)).ToList();
                goal.ProgressPercent = linked.Count == 0 ? 0
                    : (int)(linked.Count(t => t.IsCompleted) * 100.0 / linked.Count);
            }
            else
            {
                goal.ProgressPercent = goal.ManualProgressPercent;
            }

            // Populate category display fields
            if (goal.CategoryId.HasValue && categoryMap.TryGetValue(goal.CategoryId.Value, out var cat))
            {
                goal.CategoryName  = cat.Name;
                goal.CategoryEmoji = cat.Emoji;
            }
            else
            {
                goal.CategoryName  = null;
                goal.CategoryEmoji = null;
            }
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var habit in data.Habits)
        {
            habit.CompletedToday = habit.CompletionDates.Contains(today);
            (habit.CurrentStreak, habit.LongestStreak) =
                StreakCalculator.Calculate(habit.CompletionDates, habit.Frequency);
        }
    }
}
