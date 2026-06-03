using System.Text.Json.Serialization;
using GoalTracker.Shared.Enums;

namespace GoalTracker.Shared.Models;

public class Habit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
    public List<DayOfWeek> TargetDays { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsArchived { get; set; }
    public string Emoji { get; set; } = string.Empty;

    public List<DateOnly> CompletionDates { get; set; } = [];

    [JsonIgnore]
    public int CurrentStreak { get; set; }
    [JsonIgnore]
    public int LongestStreak { get; set; }
    [JsonIgnore]
    public bool CompletedToday { get; set; }
}
