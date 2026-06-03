using GoalTracker.Shared.Enums;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.MainApp.Dialogs;

public sealed partial class EditTaskDialog : ContentDialog
{
    public GoalTask Task { get; private set; }

    public EditTaskDialog(GoalTask task)
    {
        Task = new GoalTask
        {
            Id = task.Id,
            GoalId = task.GoalId,
            Title = task.Title,
            Notes = task.Notes,
            Priority = task.Priority,
            DueDate = task.DueDate,
            IsTodayTask = task.IsTodayTask,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };

        InitializeComponent();

        TitleBox.Text = Task.Title;
        NotesBox.Text = Task.Notes;
        TodayToggle.IsOn = Task.IsTodayTask;
        PriorityCombo.SelectedIndex = (int)Task.Priority;

        DueDatePicker.Date = Task.DueDate.HasValue
            ? new DateTimeOffset(Task.DueDate.Value)
            : DateTimeOffset.Now;

        PrimaryButtonClick += (_, _) =>
        {
            Task.Title = TitleBox.Text.Trim();
            Task.Notes = NotesBox.Text.Trim();
            Task.Priority = (TaskPriority)PriorityCombo.SelectedIndex;
            Task.IsTodayTask = TodayToggle.IsOn;
            Task.DueDate = DueDatePicker.Date.DateTime;
        };
    }
}
