namespace GoalTracker.Shared.Helpers;

public static class DataPaths
{
    private static readonly string BaseDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "GoalTracker");

    public static string AppDataFile => Path.Combine(BaseDir, "appdata.json");
    public static string SettingsFile => Path.Combine(BaseDir, "settings.json");

    public static void EnsureDirectoryExists() => Directory.CreateDirectory(BaseDir);
}
