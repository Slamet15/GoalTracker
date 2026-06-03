namespace GoalTracker.Shared.Models;

public class GoalMilestone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;

    /// <summary>How much of the goal this milestone represents (0–100).</summary>
    public int CompletionPercent { get; set; } = 10;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
