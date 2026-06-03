namespace GoalTracker.Shared.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3D8EF0";
    public string Emoji { get; set; } = "📁";
}
