using System.Text.Json.Serialization;

namespace GoalTracker.Shared.Models;

public class ActivityEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? GoalId { get; set; }
    public Guid? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];

    [JsonIgnore]
    public TimeSpan Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : DateTime.UtcNow - StartTime;

    [JsonIgnore]
    public bool IsRunning => !EndTime.HasValue;
}
