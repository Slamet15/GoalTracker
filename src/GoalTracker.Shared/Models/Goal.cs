using System.Text.Json.Serialization;
using GoalTracker.Shared.Enums;

namespace GoalTracker.Shared.Models;

public class Goal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>One of the 8 predefined palette colors (hex).</summary>
    public string PaletteColor { get; set; } = "#3D8EF0";

    /// <summary>Reference to a Category.Id, or null if uncategorised.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Milestones for this goal. If any exist, progress is derived from them.</summary>
    public List<GoalMilestone> Milestones { get; set; } = [];

    // Task-based progress (kept for backward compat when no milestones)
    public bool UseTasksForProgress { get; set; } = true;
    public int ManualProgressPercent { get; set; }
    public List<Guid> LinkedTaskIds { get; set; } = [];

    [JsonIgnore]
    public int ProgressPercent { get; set; }

    [JsonIgnore]
    public string? CategoryName { get; set; }

    [JsonIgnore]
    public string? CategoryEmoji { get; set; }
}
