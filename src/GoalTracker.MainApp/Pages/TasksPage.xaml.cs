using GoalTracker.MainApp.Dialogs;
using GoalTracker.MainApp.ViewModels;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp.Pages;

public sealed partial class TasksPage : Page
{
    private readonly TasksViewModel _vm = new();
    private bool _localSaving;

    public TasksPage()
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
        TodayList.ItemsSource = _vm.TodayTasks;
        AllList.ItemsSource = _vm.AllTasks;
    }

    private async void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditTaskDialog(new GoalTask()) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _localSaving = true;
            await _vm.SaveTaskAsync(dialog.Task);
            _localSaving = false;
        }
        await RefreshAsync();
    }

    private async void Task_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { Tag: GoalTask task })
        {
            _localSaving = true;
            await _vm.ToggleTaskCommand.ExecuteAsync(task);
            _localSaving = false;
            await RefreshAsync();
        }
    }

    private async void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: GoalTask task })
        {
            _localSaving = true;
            await _vm.DeleteTaskCommand.ExecuteAsync(task);
            _localSaving = false;
            await RefreshAsync();
        }
    }
}
