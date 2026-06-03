using GoalTracker.Shared.Models;
using GoalTracker.Widget.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.Widget.Views;

public sealed partial class TasksSectionView : UserControl
{
    private readonly WidgetViewModel _vm;

    public TasksSectionView()
    {
        InitializeComponent();
        _vm = (App.Current as App)!.WidgetVm;
        Refresh();
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WidgetViewModel.TodayTasks))
                Refresh();
        };
    }

    private void Refresh()
    {
        TasksList.ItemsSource = _vm.TodayTasks;
        EmptyLabel.Visibility = _vm.TodayTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void Task_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { Tag: GoalTask task })
            await _vm.ToggleTaskCommand.ExecuteAsync(task);
    }
}
