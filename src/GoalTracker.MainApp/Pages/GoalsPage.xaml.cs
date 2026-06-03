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

    public GoalsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await _vm.LoadAsync();
        GoalsList.ItemsSource = _vm.Goals;
        App.FileWatcher.DataFileChanged += OnDataChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        App.FileWatcher.DataFileChanged -= OnDataChanged;
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            await _vm.LoadAsync();
            GoalsList.ItemsSource = _vm.Goals;
        });
    }

    private async void AddGoal_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditGoalDialog(new Goal()) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await _vm.SaveGoalAsync(dialog.Goal);
            GoalsList.ItemsSource = _vm.Goals;
        }
    }

    private async void EditGoal_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Goal goal })
        {
            var dialog = new EditGoalDialog(goal) { XamlRoot = XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await _vm.SaveGoalAsync(dialog.Goal);
                GoalsList.ItemsSource = _vm.Goals;
            }
        }
    }

    private async void DeleteGoal_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Goal goal })
            await _vm.DeleteGoalCommand.ExecuteAsync(goal);
    }
}
