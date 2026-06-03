using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoalTracker.Shared.Models;

namespace GoalTracker.MainApp.ViewModels;

public partial class ActivityLogViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ActivityEntry> _entries = [];

    [ObservableProperty]
    private ActivityEntry? _runningEntry;

    [ObservableProperty]
    private bool _isLoading;

    public async Task LoadAsync()
    {
        IsLoading = true;
        var data = await App.DataService.LoadAsync();
        var sorted = data.ActivityLog.OrderByDescending(e => e.StartTime).ToList();
        Entries = new ObservableCollection<ActivityEntry>(sorted);
        RunningEntry = sorted.FirstOrDefault(e => e.IsRunning);
        IsLoading = false;
    }

    [RelayCommand]
    private async Task StartTimerAsync(string title)
    {
        var data = await App.DataService.LoadAsync();
        // Stop any running entry first
        foreach (var e in data.ActivityLog.Where(e => !e.EndTime.HasValue))
            e.EndTime = DateTime.UtcNow;

        data.ActivityLog.Add(new ActivityEntry
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
            StartTime = DateTime.UtcNow
        });

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

    [RelayCommand]
    private async Task DeleteEntryAsync(ActivityEntry entry)
    {
        var data = await App.DataService.LoadAsync();
        data.ActivityLog.RemoveAll(e => e.Id == entry.Id);
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }
}
