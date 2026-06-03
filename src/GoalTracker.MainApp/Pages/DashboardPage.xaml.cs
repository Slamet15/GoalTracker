using GoalTracker.Shared.Enums;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await RefreshAsync();
        App.FileWatcher.DataFileChanged += (_, _) =>
            DispatcherQueue.TryEnqueue(async () => await RefreshAsync());
    }

    private async Task RefreshAsync()
    {
        var data = await App.DataService.LoadAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        ActiveGoalsCount.Text = data.Goals.Count(g => g.Status == GoalStatus.Active).ToString();
        TodayTasksCount.Text = data.Tasks.Count(t => t.IsTodayTask && !t.IsCompleted).ToString();
        HabitsDoneCount.Text = data.Habits.Count(h => h.CompletionDates.Contains(today)).ToString();

        var todayMinutes = data.ActivityLog
            .Where(e => e.StartTime.Date == DateTime.Today)
            .Sum(e => (e.EndTime ?? DateTime.UtcNow).Subtract(e.StartTime).TotalMinutes);
        TimeTodayText.Text = $"{(int)todayMinutes / 60}h {(int)todayMinutes % 60}m";

        RecentGoals.ItemsSource = data.Goals
            .Where(g => g.Status == GoalStatus.Active)
            .Take(5)
            .ToList();
    }
}
