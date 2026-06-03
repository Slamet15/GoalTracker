using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoalTracker.Shared.Enums;
using GoalTracker.Shared.Models;

namespace GoalTracker.Widget.ViewModels;

public partial class WidgetViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Goal> _goals = [];
    [ObservableProperty] private ObservableCollection<GoalTask> _todayTasks = [];
    [ObservableProperty] private ObservableCollection<ActivityEntry> _todayActivity = [];
    [ObservableProperty] private ObservableCollection<Habit> _habits = [];
    [ObservableProperty] private ActivityEntry? _runningEntry;

    public async Task LoadAsync()
    {
        var data = await App.DataService.LoadAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        Goals = new ObservableCollection<Goal>(
            data.Goals.Where(g => g.Status == GoalStatus.Active).Take(5));

        TodayTasks = new ObservableCollection<GoalTask>(
            data.Tasks.Where(t => t.IsTodayTask && !t.IsCompleted));

        TodayActivity = new ObservableCollection<ActivityEntry>(
            data.ActivityLog.Where(e => e.StartTime.Date == DateTime.Today)
                            .OrderByDescending(e => e.StartTime));

        Habits = new ObservableCollection<Habit>(
            data.Habits.Where(h => !h.IsArchived));

        RunningEntry = data.ActivityLog.FirstOrDefault(e => e.IsRunning);
    }

    [RelayCommand]
    private async Task ToggleTaskAsync(GoalTask task)
    {
        var data = await App.DataService.LoadAsync();
        var target = data.Tasks.FirstOrDefault(t => t.Id == task.Id);
        if (target is null) return;
        target.IsCompleted = !target.IsCompleted;
        target.CompletedAt = target.IsCompleted ? DateTime.UtcNow : null;
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ToggleHabitAsync(Habit habit)
    {
        var data = await App.DataService.LoadAsync();
        var target = data.Habits.FirstOrDefault(h => h.Id == habit.Id);
        if (target is null) return;
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (target.CompletionDates.Contains(today))
            target.CompletionDates.Remove(today);
        else
            target.CompletionDates.Add(today);
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task StopTimerAsync()
    {
        var data = await App.DataService.LoadAsync();
        foreach (var e in data.ActivityLog.Where(e => !e.EndTime.HasValue))
            e.EndTime = DateTime.UtcNow;
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }
}
