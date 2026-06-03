using GoalTracker.Shared.Helpers;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;

namespace GoalTracker.MainApp.Pages;

public sealed partial class SettingsPage : Page
{
    private AppSettings _settings = new();
    private bool _loaded;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _settings = App.SettingsService.Load();
        DataPathText.Text = DataPaths.AppDataFile;
        OpacitySlider.Value = _settings.WidgetOpacity;
        OpacityLabel.Text = $"{(int)(_settings.WidgetOpacity * 100)}%";
        StartupToggle.IsOn = _settings.LaunchWidgetOnStartup;

        ThemeCombo.SelectedIndex = _settings.Theme switch
        {
            "Light" => 1,
            "Dark"  => 2,
            _       => 0
        };
        _loaded = true;
    }

    private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        DataPaths.EnsureDirectoryExists();
        System.Diagnostics.Process.Start("explorer.exe",
            System.IO.Path.GetDirectoryName(DataPaths.AppDataFile)!);
    }

    private void OpacitySlider_Changed(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_loaded) return;
        _settings.WidgetOpacity = e.NewValue;
        OpacityLabel.Text = $"{(int)(e.NewValue * 100)}%";
        App.SettingsService.Save(_settings);
    }

    private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;
        _settings.LaunchWidgetOnStartup = StartupToggle.IsOn;
        App.SettingsService.Save(_settings);
        SetStartupRegistration(StartupToggle.IsOn);
    }

    private void ThemeCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_loaded) return;
        _settings.Theme = ThemeCombo.SelectedIndex switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "System"
        };
        App.SettingsService.Save(_settings);
    }

    private static void SetStartupRegistration(bool enable)
    {
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string valueName = "GoalTrackerWidget";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath, writable: true);
        if (enable)
        {
            var widgetExe = System.IO.Path.Combine(
                AppContext.BaseDirectory, "GoalTracker.Widget.exe");
            key?.SetValue(valueName, widgetExe);
        }
        else
        {
            key?.DeleteValue(valueName, throwOnMissingValue: false);
        }
    }
}
