using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoalTracker.Shared.Enums;
using GoalTracker.Shared.Models;

namespace GoalTracker.MainApp.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Goal> _goals = [];

    [ObservableProperty]
    private bool _isLoading;

    public async Task LoadAsync()
    {
        IsLoading = true;
        var data = await App.DataService.LoadAsync();
        Goals = new ObservableCollection<Goal>(data.Goals.Where(g => g.Status == GoalStatus.Active));
        IsLoading = false;
    }

    [RelayCommand]
    private async Task ToggleGoalStatusAsync(Goal goal)
    {
        var data = await App.DataService.LoadAsync();
        var target = data.Goals.FirstOrDefault(g => g.Id == goal.Id);
        if (target is null) return;

        target.Status = target.Status == GoalStatus.Active ? GoalStatus.Completed : GoalStatus.Active;
        if (target.Status == GoalStatus.Completed)
            target.CompletedAt = DateTime.UtcNow;

        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteGoalAsync(Goal goal)
    {
        var data = await App.DataService.LoadAsync();
        data.Goals.RemoveAll(g => g.Id == goal.Id);
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }

    public async Task SaveGoalAsync(Goal goal)
    {
        var data = await App.DataService.LoadAsync();
        var existing = data.Goals.FirstOrDefault(g => g.Id == goal.Id);
        if (existing is null)
            data.Goals.Add(goal);
        else
        {
            var idx = data.Goals.IndexOf(existing);
            data.Goals[idx] = goal;
        }
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }
}
