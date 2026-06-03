using GoalTracker.MainApp.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Goal Tracker";
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 720));
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag as string;
            Type? pageType = tag switch
            {
                "dashboard" => typeof(DashboardPage),
                "goals"     => typeof(GoalsPage),
                "tasks"     => typeof(TasksPage),
                "habits"    => typeof(HabitsPage),
                "activity"  => typeof(ActivityLogPage),
                _           => null
            };

            if (pageType is not null)
                ContentFrame.Navigate(pageType);
        }
    }
}
