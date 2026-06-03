using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.MainApp.Dialogs;

public sealed partial class EditGoalDialog : ContentDialog
{
    public Goal Goal { get; private set; }

    public Visibility ManualProgressVisibility =>
        UseTasksToggle?.IsOn == false ? Visibility.Visible : Visibility.Collapsed;

    public EditGoalDialog(Goal goal)
    {
        Goal = new Goal
        {
            Id = goal.Id,
            Title = goal.Title,
            Description = goal.Description,
            Category = goal.Category,
            Color = goal.Color,
            DueDate = goal.DueDate,
            UseTasksForProgress = goal.UseTasksForProgress,
            ManualProgressPercent = goal.ManualProgressPercent,
            LinkedTaskIds = goal.LinkedTaskIds,
            Status = goal.Status
        };

        InitializeComponent();

        TitleBox.Text = Goal.Title;
        DescBox.Text = Goal.Description;
        CategoryBox.Text = Goal.Category;
        ColorBox.Text = Goal.Color;
        UseTasksToggle.IsOn = Goal.UseTasksForProgress;
        ManualProgress.Value = Goal.ManualProgressPercent;

        DueDatePicker.Date = Goal.DueDate.HasValue
            ? new DateTimeOffset(Goal.DueDate.Value)
            : DateTimeOffset.Now;

        UseTasksToggle.Toggled += (_, _) =>
        {
            ManualProgress.Visibility = UseTasksToggle.IsOn
                ? Visibility.Collapsed
                : Visibility.Visible;
        };

        PrimaryButtonClick += (_, _) =>
        {
            Goal.Title = TitleBox.Text.Trim();
            Goal.Description = DescBox.Text.Trim();
            Goal.Category = CategoryBox.Text.Trim();
            Goal.Color = string.IsNullOrWhiteSpace(ColorBox.Text) ? "#0078D4" : ColorBox.Text.Trim();
            Goal.UseTasksForProgress = UseTasksToggle.IsOn;
            Goal.ManualProgressPercent = (int)ManualProgress.Value;
            Goal.DueDate = DueDatePicker.Date.DateTime;
        };
    }
}
