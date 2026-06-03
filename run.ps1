# GoalTracker Launcher
# Usage:
#   .\run.ps1           - build all & run both apps
#   .\run.ps1 -app      - run main app only
#   .\run.ps1 -widget   - run widget only
#   .\run.ps1 -nobuild  - skip build, just launch
#   .\run.ps1 -install  - register both apps to start on Windows login
#   .\run.ps1 -uninstall - remove from Windows startup

param(
    [switch]$app,
    [switch]$widget,
    [switch]$nobuild,
    [switch]$install,
    [switch]$uninstall
)

$Root     = $PSScriptRoot
$Config   = "Debug"
$Platform = "x64"
$Tfm      = "net9.0-windows10.0.19041.0"
$OutBase  = "$Root\src\{0}\bin\$Platform\$Config\$Tfm"

$MainExe   = ($OutBase -f "GoalTracker.MainApp") + "\GoalTracker.MainApp.exe"
$WidgetExe = ($OutBase -f "GoalTracker.Widget")  + "\GoalTracker.Widget.exe"

# ── Install startup tasks ─────────────────────────────────────────────────────
if ($install) {
    Write-Host "Registering GoalTracker apps to run at Windows login..." -ForegroundColor Cyan

    # Build first so the exes exist
    dotnet build "$Root\GoalTracker.sln" -c $Config
    if ($LASTEXITCODE -ne 0) { Write-Host "Build failed." -ForegroundColor Red; exit 1 }

    # Register Main App as a scheduled task (at logon, no console window)
    $actions = @(
        New-ScheduledTaskAction -Execute $MainExe,
        New-ScheduledTaskAction -Execute $WidgetExe
    )
    $trigger  = New-ScheduledTaskTrigger -AtLogOn
    $settings = New-ScheduledTaskSettingsSet -ExecutionTimeLimit 0 -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

    Register-ScheduledTask -TaskName "GoalTracker.MainApp" `
        -Action (New-ScheduledTaskAction -Execute $MainExe) `
        -Trigger $trigger -Settings $settings `
        -RunLevel Limited -Force | Out-Null

    Register-ScheduledTask -TaskName "GoalTracker.Widget" `
        -Action (New-ScheduledTaskAction -Execute $WidgetExe) `
        -Trigger $trigger -Settings $settings `
        -RunLevel Limited -Force | Out-Null

    Write-Host "Done! Both apps will now launch automatically on login." -ForegroundColor Green
    Write-Host "To remove: .\run.ps1 -uninstall" -ForegroundColor Gray
    exit 0
}

# ── Uninstall startup tasks ───────────────────────────────────────────────────
if ($uninstall) {
    Write-Host "Removing GoalTracker from Windows startup..." -ForegroundColor Cyan
    Unregister-ScheduledTask -TaskName "GoalTracker.MainApp" -Confirm:$false -ErrorAction SilentlyContinue
    Unregister-ScheduledTask -TaskName "GoalTracker.Widget"  -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "Done. Apps will no longer launch on login." -ForegroundColor Green
    exit 0
}

# ── Build ─────────────────────────────────────────────────────────────────────
if (-not $nobuild) {
    # Stop running instances first to release file locks
    Get-Process -Name "GoalTracker.MainApp", "GoalTracker.Widget" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Milliseconds 500

    Write-Host "Building..." -ForegroundColor Cyan

    if ($app) {
        dotnet build "$Root\src\GoalTracker.MainApp\GoalTracker.MainApp.csproj" -c $Config
    } elseif ($widget) {
        dotnet build "$Root\src\GoalTracker.Widget\GoalTracker.Widget.csproj" -c $Config
    } else {
        dotnet build "$Root\GoalTracker.sln" -c $Config
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed." -ForegroundColor Red
        exit 1
    }
    Write-Host "Build succeeded." -ForegroundColor Green
}

# ── Launch ────────────────────────────────────────────────────────────────────
if ($app) {
    Write-Host "Launching Main App..." -ForegroundColor Cyan
    Start-Process $MainExe
} elseif ($widget) {
    Write-Host "Launching Widget..." -ForegroundColor Cyan
    Start-Process $WidgetExe
} else {
    Write-Host "Launching both apps..." -ForegroundColor Cyan
    Start-Process $MainExe
    Start-Process $WidgetExe
}

Write-Host "Done." -ForegroundColor Green
