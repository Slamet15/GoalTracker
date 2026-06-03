using GoalTracker.Shared.Enums;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml.Controls;

namespace GoalTracker.MainApp.Dialogs;

public sealed partial class EditHabitDialog : ContentDialog
{
    public Habit Habit { get; private set; }

    public EditHabitDialog(Habit habit)
    {
        Habit = new Habit
        {
            Id = habit.Id,
            Title = habit.Title,
            Description = habit.Description,
            Emoji = habit.Emoji,
            Frequency = habit.Frequency,
            CreatedAt = habit.CreatedAt,
            CompletionDates = habit.CompletionDates
        };

        InitializeComponent();

        TitleBox.Text = Habit.Title;
        DescBox.Text = Habit.Description;
        EmojiBox.Text = Habit.Emoji;
        FrequencyCombo.SelectedIndex = (int)Habit.Frequency;

        PrimaryButtonClick += (_, _) =>
        {
            Habit.Title = TitleBox.Text.Trim();
            Habit.Description = DescBox.Text.Trim();
            Habit.Emoji = EmojiBox.Text.Trim();
            Habit.Frequency = FrequencyCombo.SelectedIndex == 1
                ? HabitFrequency.Weekly
                : HabitFrequency.Daily;
        };
    }
}
