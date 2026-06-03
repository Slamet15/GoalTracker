namespace GoalTracker.Shared.Models;

public class AppData
{
    public int SchemaVersion { get; set; } = 1;
    public DateTime LastModified { get; set; }
    public List<Goal> Goals { get; set; } = [];
    public List<GoalTask> Tasks { get; set; } = [];
    public List<Habit> Habits { get; set; } = [];
    public List<ActivityEntry> ActivityLog { get; set; } = [];
}
