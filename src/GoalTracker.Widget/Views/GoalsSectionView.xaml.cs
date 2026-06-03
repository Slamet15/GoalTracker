using GoalTracker.Widget.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.Widget.Views;

public sealed partial class GoalsSectionView : UserControl
{
    private readonly WidgetViewModel _vm;

    public GoalsSectionView()
    {
        InitializeComponent();
        _vm = (App.Current as App)!.WidgetVm;
        GoalsList.ItemsSource = _vm.Goals;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WidgetViewModel.Goals))
                GoalsList.ItemsSource = _vm.Goals;
        };
    }
}
