using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoalTracker.Shared.Models;

namespace GoalTracker.MainApp.ViewModels;

public partial class HabitsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Habit> _habits = [];

    [ObservableProperty]
    private bool _isLoading;

    public async Task LoadAsync()
    {
        IsLoading = true;
        var data = await App.DataService.LoadAsync();
        Habits = new ObservableCollection<Habit>(data.Habits.Where(h => !h.IsArchived));
        IsLoading = false;
    }

    [RelayCommand]
    private async Task ToggleHabitTodayAsync(Habit habit)
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

    public async Task SaveHabitAsync(Habit habit)
    {
        var data = await App.DataService.LoadAsync();
        var existing = data.Habits.FirstOrDefault(h => h.Id == habit.Id);
        if (existing is null)
            data.Habits.Add(habit);
        else
        {
            var idx = data.Habits.IndexOf(existing);
            data.Habits[idx] = habit;
        }
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }
}
