using GoalTracker.Shared.Models;
using GoalTracker.Widget.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.Widget.Views;

public sealed partial class HabitsSectionView : UserControl
{
    private readonly WidgetViewModel _vm;

    public HabitsSectionView()
    {
        InitializeComponent();
        _vm = (App.Current as App)!.WidgetVm;
        HabitsList.ItemsSource = _vm.Habits;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WidgetViewModel.Habits))
                HabitsList.ItemsSource = _vm.Habits;
        };
    }

    private async void Habit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Habit habit })
            await _vm.ToggleHabitCommand.ExecuteAsync(habit);
    }
}
