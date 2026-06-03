namespace GoalTracker.Shared.Models;

public class AppSettings
{
    public int WidgetX { get; set; }
    public int WidgetY { get; set; }
    public int WidgetWidth { get; set; } = 320;
    public int WidgetHeight { get; set; } = 600;
    public double WidgetOpacity { get; set; } = 0.85;
    public bool WidgetCollapsed { get; set; }
    public string Theme { get; set; } = "System";
    public bool LaunchWidgetOnStartup { get; set; }
}
