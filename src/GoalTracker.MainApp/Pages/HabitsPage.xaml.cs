using GoalTracker.MainApp.Dialogs;
using GoalTracker.MainApp.ViewModels;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp.Pages;

public sealed partial class HabitsPage : Page
{
    private readonly HabitsViewModel _vm = new();
    private bool _localSaving;

    public HabitsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await RefreshAsync();
        App.FileWatcher.DataFileChanged += OnDataChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        App.FileWatcher.DataFileChanged -= OnDataChanged;
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        if (_localSaving) return;
        DispatcherQueue.TryEnqueue(async () => await RefreshAsync());
    }

    private async Task RefreshAsync()
    {
        await _vm.LoadAsync();
        HabitsList.ItemsSource = _vm.Habits;
    }

    private async void AddHabit_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditHabitDialog(new Habit()) { XamlRoot = XamlRoot };
        await dialog.ShowAsync(); // always refresh regardless of result
        _localSaving = true;
        if (dialog.Saved) await _vm.SaveHabitAsync(dialog.Habit);
        _localSaving = false;
        await RefreshAsync();
    }

    private async void EditHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Habit habit })
        {
            var dialog = new EditHabitDialog(habit) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();
            _localSaving = true;
            if (dialog.Saved) await _vm.SaveHabitAsync(dialog.Habit);
            _localSaving = false;
            await RefreshAsync();
        }
    }

    private async void ToggleHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Habit habit })
        {
            _localSaving = true;
            await _vm.ToggleHabitTodayCommand.ExecuteAsync(habit);
            _localSaving = false;
            await RefreshAsync();
        }
    }

    private async void DeleteHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Habit habit })
        {
            _localSaving = true;
            var data = await App.DataService.LoadAsync();
            data.Habits.RemoveAll(h => h.Id == habit.Id);
            await App.DataService.SaveAsync(data);
            _localSaving = false;
            await RefreshAsync();
        }
    }
}
