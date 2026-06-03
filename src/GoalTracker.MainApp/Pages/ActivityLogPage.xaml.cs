using GoalTracker.MainApp.ViewModels;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp.Pages;

public sealed partial class ActivityLogPage : Page
{
    private readonly ActivityLogViewModel _vm = new();

    public ActivityLogPage()
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
        ActivityList.ItemsSource = _vm.Entries;
        var running = _vm.RunningEntry;
        StopBtn.IsEnabled = running is not null;
        RunningLabel.Text = running is not null ? $"Running: {running.Title}" : string.Empty;
    }

    private async void StartTimer_Click(object sender, RoutedEventArgs e)
    {
        await _vm.StartTimerCommand.ExecuteAsync(TimerTitle.Text);
        TimerTitle.Text = string.Empty;
    }

    private async void StopTimer_Click(object sender, RoutedEventArgs e) =>
        await _vm.StopTimerCommand.ExecuteAsync(null);

    private async void DeleteEntry_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ActivityEntry entry })
            await _vm.DeleteEntryCommand.ExecuteAsync(entry);
    }
}
