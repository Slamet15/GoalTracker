using System.Collections.ObjectModel;
using GoalTracker.Shared.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace GoalTracker.MainApp.Dialogs;

public sealed partial class EditGoalDialog : ContentDialog
{
    public Goal Goal { get; private set; }

    private static readonly string[] Palette =
    [
        "#3D8EF0", "#4CAF50", "#FF6B35", "#9C27B0",
        "#F44336", "#00BCD4", "#FF9800", "#607D8B"
    ];

    private string _selectedColor;
    private readonly List<Ellipse> _colorDots = [];
    private readonly ObservableCollection<GoalMilestone> _milestones;

    public EditGoalDialog(Goal goal)
    {
        Goal = new Goal
        {
            Id                    = goal.Id,
            Title                 = goal.Title,
            Description           = goal.Description,
            PaletteColor          = string.IsNullOrEmpty(goal.PaletteColor) ? Palette[0] : goal.PaletteColor,
            CategoryId            = goal.CategoryId,
            DueDate               = goal.DueDate,
            UseTasksForProgress   = goal.UseTasksForProgress,
            ManualProgressPercent = goal.ManualProgressPercent,
            LinkedTaskIds         = goal.LinkedTaskIds,
            Status                = goal.Status,
            Milestones            = goal.Milestones.Select(m => new GoalMilestone
            {
                Id                = m.Id,
                Title             = m.Title,
                CompletionPercent = m.CompletionPercent,
                IsCompleted       = m.IsCompleted,
                CompletedAt       = m.CompletedAt
            }).ToList()
        };

        _selectedColor = Goal.PaletteColor;
        _milestones    = new ObservableCollection<GoalMilestone>(Goal.Milestones);

        InitializeComponent();

        TitleBox.Text        = Goal.Title;
        DescBox.Text         = Goal.Description;
        ManualProgress.Value = Goal.ManualProgressPercent;
        DueDatePicker.Date   = Goal.DueDate.HasValue
            ? new DateTimeOffset(Goal.DueDate.Value)
            : DateTimeOffset.Now;

        // Progress mode selection
        if (Goal.Milestones.Count > 0)
            ProgressModeCombo.SelectedIndex = 0;
        else if (Goal.UseTasksForProgress)
            ProgressModeCombo.SelectedIndex = 1;
        else
            ProgressModeCombo.SelectedIndex = 2;

        BuildPalette();
        MilestonesList.ItemsSource = _milestones;
        UpdateTotalPercent();
        _ = LoadCategoriesAsync();

        PrimaryButtonClick += (_, _) => CollectValues();
    }

    // ── Category loading ──────────────────────────────────────────────────────
    private async Task LoadCategoriesAsync()
    {
        var data = await App.DataService.LoadAsync();

        CategoryCombo.Items.Clear();
        CategoryCombo.Items.Add(new ComboBoxItem { Content = "— No category —", Tag = (Guid?)null });

        foreach (var cat in data.Categories)
        {
            CategoryCombo.Items.Add(new ComboBoxItem
            {
                Tag     = (Guid?)cat.Id,
                Content = $"{cat.Emoji} {cat.Name}"
            });
        }

        // Restore selection
        for (int i = 0; i < CategoryCombo.Items.Count; i++)
        {
            if (CategoryCombo.Items[i] is ComboBoxItem { Tag: Guid id } && id == Goal.CategoryId)
            {
                CategoryCombo.SelectedIndex = i;
                return;
            }
        }
        CategoryCombo.SelectedIndex = 0;
    }

    // ── Palette ───────────────────────────────────────────────────────────────
    private void BuildPalette()
    {
        foreach (var hex in Palette)
        {
            var dot = new Ellipse { Width = 26, Height = 26, Fill = HexBrush(hex) };
            if (hex == _selectedColor) { dot.Stroke = new SolidColorBrush(Colors.White); dot.StrokeThickness = 2; }
            string c = hex;
            dot.PointerPressed += (_, _) => SelectColor(c);
            PalettePanel.Children.Add(dot);
            _colorDots.Add(dot);
        }
    }

    private void SelectColor(string hex)
    {
        _selectedColor = hex;
        for (int i = 0; i < Palette.Length; i++)
        {
            _colorDots[i].Stroke          = Palette[i] == hex ? new SolidColorBrush(Colors.White) : null;
            _colorDots[i].StrokeThickness = Palette[i] == hex ? 2 : 0;
        }
    }

    // ── Progress mode ─────────────────────────────────────────────────────────
    private void ProgressModeCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ManualProgress is null || MilestonesPanel is null) return;
        int idx = ProgressModeCombo.SelectedIndex;
        ManualProgress.Visibility  = idx == 2 ? Visibility.Visible : Visibility.Collapsed;
        MilestonesPanel.Visibility = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Milestones ────────────────────────────────────────────────────────────
    private void AddMilestone_Click(object sender, RoutedEventArgs e)
    {
        _milestones.Add(new GoalMilestone { Title = "", CompletionPercent = 10 });
        UpdateTotalPercent();
    }

    private void DeleteMilestone_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: GoalMilestone m })
        {
            _milestones.Remove(m);
            UpdateTotalPercent();
        }
    }

    private void UpdateTotalPercent()
    {
        int total = _milestones.Sum(m => m.CompletionPercent);
        TotalPercentLabel.Text    = $"{total}% total";
        PercentWarning.Visibility = total > 100 ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Collect on save ───────────────────────────────────────────────────────
    private void CollectValues()
    {
        Goal.Title                = TitleBox.Text.Trim();
        Goal.Description          = DescBox.Text.Trim();
        Goal.PaletteColor         = _selectedColor;
        Goal.DueDate              = DueDatePicker.Date.DateTime;
        Goal.ManualProgressPercent = (int)ManualProgress.Value;

        int mode = ProgressModeCombo.SelectedIndex;
        Goal.UseTasksForProgress = mode == 1;
        Goal.Milestones          = mode == 0 ? [.. _milestones] : [];

        Goal.CategoryId = CategoryCombo.SelectedItem is ComboBoxItem { Tag: Guid id } ? id : null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static SolidColorBrush HexBrush(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex[..2], 16);
        byte g = Convert.ToByte(hex[2..4], 16);
        byte b = Convert.ToByte(hex[4..6], 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
    }
}
