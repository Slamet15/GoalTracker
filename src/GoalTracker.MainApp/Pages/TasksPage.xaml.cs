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

    public TasksPage()
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

    private async void OnDataChanged(object? sender, EventArgs e) =>
        await DispatcherQueue.TryEnqueue(async () => await LoadAsync());

    private async Task LoadAsync()
    {
        await _vm.LoadAsync();
        TodayList.ItemsSource = _vm.TodayTasks;
        AllList.ItemsSource = _vm.AllTasks;
    }

    private async void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditTaskDialog(new GoalTask()) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await _vm.SaveTaskAsync(dialog.Task);
    }

    private async void Task_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { Tag: GoalTask task })
            await _vm.ToggleTaskCommand.ExecuteAsync(task);
    }

    private async void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: GoalTask task })
            await _vm.DeleteTaskCommand.ExecuteAsync(task);
    }
}
