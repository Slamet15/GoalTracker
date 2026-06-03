using GoalTracker.Shared.Enums;
using GoalTracker.Shared.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

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

        // Stat cards
        ActiveGoalsCount.Text = data.Goals.Count(g => g.Status == GoalStatus.Active).ToString();
        TodayTasksCount.Text  = data.Tasks.Count(t => t.IsTodayTask && !t.IsCompleted).ToString();
        HabitsDoneCount.Text  = data.Habits.Count(h => h.CompletionDates.Contains(today)).ToString();

        var todayMinutes = data.ActivityLog
            .Where(e => e.StartTime.Date == DateTime.Today)
            .Sum(e => (e.EndTime ?? DateTime.UtcNow).Subtract(e.StartTime).TotalMinutes);
        TimeTodayText.Text = $"{(int)todayMinutes / 60}h {(int)todayMinutes % 60}m";

        // Horizontal goal cards
        RecentGoalsPanel.Children.Clear();
        foreach (var goal in data.Goals.Where(g => g.Status == GoalStatus.Active).Take(6))
            RecentGoalsPanel.Children.Add(BuildGoalCard(goal));

        // Today items (tasks + habits)
        var todayItems = new List<string>();
        todayItems.AddRange(data.Tasks
            .Where(t => t.IsTodayTask && !t.IsCompleted)
            .Select(t => $"☐  {t.Title}"));
        todayItems.AddRange(data.Habits
            .Where(h => !h.IsArchived && !h.CompletedToday)
            .Select(h => $"{h.Emoji}  {h.Title}"));
        TodayItems.ItemsSource = todayItems;
    }

    private static Border BuildGoalCard(Goal goal)
    {
        var titleBlock = new TextBlock
        {
            Text       = goal.Title,
            FontSize   = 13,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 232, 232)),
            TextWrapping = TextWrapping.Wrap,
            MaxLines   = 2
        };

        var progressBar = new ProgressBar
        {
            Value   = goal.ProgressPercent,
            Maximum = 100,
            Height  = 3,
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 37, 37, 37)),
            Foreground = HexBrush(goal.PaletteColor),
            BorderThickness = new Thickness(0)
        };

        var percentLabel = new TextBlock
        {
            FontSize   = 11,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 85, 85, 85))
        };
        var run1 = new Microsoft.UI.Xaml.Documents.Run { Text = goal.ProgressPercent.ToString() };
        var run2 = new Microsoft.UI.Xaml.Documents.Run { Text = "%" };
        percentLabel.Inlines.Add(run1);
        percentLabel.Inlines.Add(run2);

        var colorBar = new Rectangle
        {
            Width  = 3,
            Fill   = HexBrush(goal.PaletteColor),
            RadiusX = 2, RadiusY = 2
        };

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(titleBlock);
        content.Children.Add(progressBar);
        content.Children.Add(percentLabel);

        var inner = new Grid { ColumnSpacing = 10 };
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(colorBar, 0);
        Grid.SetColumn(content, 1);
        inner.Children.Add(colorBar);
        inner.Children.Add(content);

        return new Border
        {
            Width           = 180,
            Background      = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 28, 28, 28)),
            BorderBrush     = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 42, 42, 42)),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(8),
            Padding         = new Thickness(14, 12, 14, 12),
            Child           = inner
        };
    }

    private static SolidColorBrush HexBrush(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6) return new SolidColorBrush(Colors.Gray);
        byte r = Convert.ToByte(hex[..2], 16);
        byte g = Convert.ToByte(hex[2..4], 16);
        byte b = Convert.ToByte(hex[4..6], 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
    }
}
