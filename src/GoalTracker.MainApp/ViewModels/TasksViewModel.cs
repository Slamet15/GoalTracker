using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoalTracker.Shared.Models;

namespace GoalTracker.MainApp.ViewModels;

public partial class TasksViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GoalTask> _todayTasks = [];

    [ObservableProperty]
    private ObservableCollection<GoalTask> _allTasks = [];

    [ObservableProperty]
    private bool _isLoading;

    public async Task LoadAsync()
    {
        IsLoading = true;
        var data = await App.DataService.LoadAsync();
        var today = data.Tasks.Where(t => t.IsTodayTask && !t.IsCompleted).ToList();
        TodayTasks = new ObservableCollection<GoalTask>(today);
        AllTasks = new ObservableCollection<GoalTask>(data.Tasks.OrderByDescending(t => t.CreatedAt));
        IsLoading = false;
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
    private async Task DeleteTaskAsync(GoalTask task)
    {
        var data = await App.DataService.LoadAsync();
        data.Tasks.RemoveAll(t => t.Id == task.Id);
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }

    public async Task SaveTaskAsync(GoalTask task)
    {
        var data = await App.DataService.LoadAsync();
        var existing = data.Tasks.FirstOrDefault(t => t.Id == task.Id);
        if (existing is null)
            data.Tasks.Add(task);
        else
        {
            var idx = data.Tasks.IndexOf(existing);
            data.Tasks[idx] = task;
        }
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }
}
