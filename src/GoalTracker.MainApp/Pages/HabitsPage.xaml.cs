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

    public HabitsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadAsync();
        App.FileWatcher.DataFileChanged += OnDataChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        App.FileWatcher.DataFileChanged -= OnDataChanged;
    }

    private void OnDataChanged(object? sender, EventArgs e) =>
        DispatcherQueue.TryEnqueue(async () => await LoadAsync());

    private async Task LoadAsync()
    {
        await _vm.LoadAsync();
        HabitsList.ItemsSource = _vm.Habits;
    }

    private async void AddHabit_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditHabitDialog(new Habit()) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await _vm.SaveHabitAsync(dialog.Habit);
    }

    private async void ToggleHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Habit habit })
            await _vm.ToggleHabitTodayCommand.ExecuteAsync(habit);
    }

    private async void DeleteHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Habit habit })
        {
            var data = await App.DataService.LoadAsync();
            data.Habits.RemoveAll(h => h.Id == habit.Id);
            await App.DataService.SaveAsync(data);
            await LoadAsync();
        }
    }
}
