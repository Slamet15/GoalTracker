using GoalTracker.Shared.Enums;
using GoalTracker.Shared.Helpers;
using Xunit;

namespace GoalTracker.Tests;

public class StreakCalculatorTests
{
    [Fact]
    public void EmptyDates_ReturnsZero()
    {
        var (current, longest) = StreakCalculator.Calculate([], HabitFrequency.Daily);
        Assert.Equal(0, current);
        Assert.Equal(0, longest);
    }

    [Fact]
    public void SingleEntryToday_ReturnsOneOne()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var (current, longest) = StreakCalculator.Calculate([today], HabitFrequency.Daily);
        Assert.Equal(1, current);
        Assert.Equal(1, longest);
    }

    [Fact]
    public void ConsecutiveDaysEndingToday_ReturnsCorrectStreak()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dates = new List<DateOnly> { today, today.AddDays(-1), today.AddDays(-2) };
        var (current, longest) = StreakCalculator.Calculate(dates, HabitFrequency.Daily);
        Assert.Equal(3, current);
        Assert.Equal(3, longest);
    }

    [Fact]
    public void GapInStreak_CurrentIsZeroAfterGap()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        // Completed 3 days ago and 4 days ago — gap from today
        var dates = new List<DateOnly> { today.AddDays(-3), today.AddDays(-4) };
        var (current, _) = StreakCalculator.Calculate(dates, HabitFrequency.Daily);
        Assert.Equal(0, current);
    }

    [Fact]
    public void LongestStreakIsPreservedAfterBreak()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        // Long streak a week ago, short streak now
        var dates = new List<DateOnly>
        {
            today,
            today.AddDays(-10),
            today.AddDays(-11),
            today.AddDays(-12),
            today.AddDays(-13),
            today.AddDays(-14)
        };
        var (current, longest) = StreakCalculator.Calculate(dates, HabitFrequency.Daily);
        Assert.Equal(1, current);
        Assert.Equal(5, longest);
    }
}
