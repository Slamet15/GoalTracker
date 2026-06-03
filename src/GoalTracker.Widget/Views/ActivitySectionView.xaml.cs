using GoalTracker.Widget.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.Widget.Views;

public sealed partial class ActivitySectionView : UserControl
{
    private readonly WidgetViewModel _vm;

    public ActivitySectionView()
    {
        InitializeComponent();
        _vm = (App.Current as App)!.WidgetVm;
        Refresh();
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(WidgetViewModel.TodayActivity)
                              or nameof(WidgetViewModel.RunningEntry))
                Refresh();
        };
    }

    private void Refresh()
    {
        ActivityList.ItemsSource = _vm.TodayActivity;
        var running = _vm.RunningEntry;
        RunningBorder.Visibility = running is not null ? Visibility.Visible : Visibility.Collapsed;
        if (running is not null)
            RunningLabel.Text = running.Title;
    }

    private async void Stop_Click(object sender, RoutedEventArgs e) =>
        await _vm.StopTimerCommand.ExecuteAsync(null);
}
