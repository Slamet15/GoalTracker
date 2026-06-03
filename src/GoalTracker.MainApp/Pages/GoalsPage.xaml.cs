using GoalTracker.MainApp.Dialogs;
using GoalTracker.MainApp.ViewModels;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp.Pages;

public sealed partial class GoalsPage : Page
{
    private readonly GoalsViewModel _vm = new();
    private bool _localSaving; // debounce FileWatcher during local saves

    public GoalsPage()
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
        if (_localSaving) return; // skip — we reload ourselves after save
        DispatcherQueue.TryEnqueue(async () => await RefreshAsync());
    }

    private async Task RefreshAsync()
    {
        await _vm.LoadAsync();
        GoalsList.ItemsSource = _vm.Goals;
    }

    private async void AddGoal_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditGoalDialog(new Goal()) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _localSaving = true;
            await _vm.SaveGoalAsync(dialog.Goal);
            _localSaving = false;
        }
        // Always refresh — even on cancel, to catch any state changes
        await RefreshAsync();
    }

    private async void EditGoal_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Goal goal })
        {
            var dialog = new EditGoalDialog(goal) { XamlRoot = XamlRoot };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _localSaving = true;
                await _vm.SaveGoalAsync(dialog.Goal);
                _localSaving = false;
            }
            await RefreshAsync();
        }
    }

    private async void DeleteGoal_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Goal goal })
        {
            _localSaving = true;
            await _vm.DeleteGoalCommand.ExecuteAsync(goal);
            _localSaving = false;
            await RefreshAsync();
        }
    }
}
