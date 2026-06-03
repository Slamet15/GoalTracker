using GoalTracker.Shared.Enums;

namespace GoalTracker.Shared.Models;

public class GoalTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? GoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public bool IsTodayTask { get; set; }
    public List<string> Tags { get; set; } = [];
}
