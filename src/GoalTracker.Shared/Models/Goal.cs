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
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";

    public bool UseTasksForProgress { get; set; } = true;
    public int ManualProgressPercent { get; set; }

    public List<Guid> LinkedTaskIds { get; set; } = [];

    [JsonIgnore]
    public int ProgressPercent { get; set; }
}
