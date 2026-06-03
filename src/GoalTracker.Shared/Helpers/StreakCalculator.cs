using GoalTracker.Shared.Enums;

namespace GoalTracker.Shared.Helpers;

public static class StreakCalculator
{
    public static (int current, int longest) Calculate(
        IReadOnlyList<DateOnly> dates, HabitFrequency frequency)
    {
        if (dates.Count == 0) return (0, 0);

        var sorted = dates.OrderByDescending(d => d).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Streak is only live if completed today or yesterday
        if (sorted[0] < today.AddDays(-1))
            return (0, CalculateLongest(sorted, frequency));

        int running = 1, longest = 1;
        for (int i = 1; i < sorted.Count; i++)
        {
            int gap = sorted[i - 1].DayNumber - sorted[i].DayNumber;
            bool continuous = frequency == HabitFrequency.Daily ? gap == 1 : gap <= 7;
            if (continuous)
            {
                running++;
                longest = Math.Max(longest, running);
            }
            else
            {
                break;
            }
        }

        return (running, Math.Max(longest, CalculateLongest(sorted, frequency)));
    }

    private static int CalculateLongest(List<DateOnly> sorted, HabitFrequency frequency)
    {
        if (sorted.Count == 0) return 0;
        int longest = 1, running = 1;
        for (int i = 1; i < sorted.Count; i++)
        {
            int gap = sorted[i - 1].DayNumber - sorted[i].DayNumber;
            bool continuous = frequency == HabitFrequency.Daily ? gap == 1 : gap <= 7;
            if (continuous)
                longest = Math.Max(longest, ++running);
            else
                running = 1;
        }
        return longest;
    }
}
