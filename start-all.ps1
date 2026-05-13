#Requires -Version 5.1
<#
.SYNOPSIS
    Starts the OCAS Tracker API (new window) and UI (current window).
.DESCRIPTION
    Checks all prerequisites, applies any pending database migrations,
    launches the ASP.NET Core API in a new terminal window on
    http://localhost:5200, then starts the Vite dev server on
    http://localhost:5173 in this window.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ApiDir       = Join-Path $PSScriptRoot "src\api"
$UiDir        = Join-Path $PSScriptRoot "src\ui"
$ApiProject   = Join-Path $ApiDir "TaskTracker.Api"
$InfraProject = "TaskTracker.Infrastructure"
$StartupProj  = "TaskTracker.Api"
$ApiUrl       = "http://localhost:5200"
$SwaggerUrl   = "$ApiUrl/swagger"
$UiUrl        = "http://localhost:5173"

# ── Helpers ────────────────────────────────────────────────────────────────────

function Write-Banner {
    Write-Host ""
    Write-Host "  ============================================" -ForegroundColor Cyan
    Write-Host "     OCAS Tracker  -  Full Stack Start       " -ForegroundColor Cyan
    Write-Host "  ============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Ok($msg)   { Write-Host "  [OK] $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "  [X]  $msg" -ForegroundColor Red }
function Write-Info($msg) { Write-Host "  [>]  $msg" -ForegroundColor Cyan }
function Write-Hint($msg) { Write-Host "       $msg" -ForegroundColor DarkGray }
function Write-Warn($msg) { Write-Host "  [!]  $msg" -ForegroundColor Yellow }

function Stop-WithError([string]$Message, [string[]]$Hints) {
    Write-Host ""
    Write-Fail $Message
    if ($Hints) {
        foreach ($h in $Hints) { Write-Hint $h }
    }
    Write-Host ""
    exit 1
}

# ── Prerequisite checks ────────────────────────────────────────────────────────

function Test-DotNet {
    Write-Info "Checking .NET SDK..."

    $ver = $null
    try { $ver = & dotnet --version 2>$null } catch { }

    if (-not $ver) {
        Stop-WithError ".NET SDK is not installed or not on PATH." @(
            "Download .NET 9 SDK from: https://dotnet.microsoft.com/download",
            "After installing, restart this terminal and try again."
        )
    }

    $major = ($ver -split '\.')[0]
    if ($major -ne '9') {
        Stop-WithError ".NET 9 SDK is required, but found version $ver." @(
            "Download .NET 9 SDK from: https://dotnet.microsoft.com/download",
            "Multiple SDK versions can coexist side by side."
        )
    }

    Write-Ok ".NET SDK $ver"
}

function Test-EfTool {
    Write-Info "Checking EF Core CLI (dotnet-ef)..."

    $efVer = $null
    try { $efVer = & dotnet ef --version 2>$null } catch { }

    if (-not $efVer) {
        Stop-WithError "EF Core CLI (dotnet-ef) is not installed." @(
            "Run: dotnet tool install --global dotnet-ef",
            "Then restart this terminal and try again."
        )
    }

    Write-Ok "EF Core CLI found"
}

function Test-Node {
    Write-Info "Checking Node.js..."

    $ver = $null
    try { $ver = & node --version 2>$null } catch { }

    if (-not $ver) {
        Stop-WithError "Node.js is not installed or not on PATH." @(
            "Download Node.js (LTS) from: https://nodejs.org",
            "After installing, restart this terminal and try again."
        )
    }

    $major = [int](($ver -replace 'v', '').Split('.')[0])
    if ($major -lt 18) {
        Stop-WithError "Node.js 18 or later is required (found $ver)." @(
            "Download the latest LTS from: https://nodejs.org"
        )
    }

    Write-Ok "Node.js $ver"
}

function Test-Npm {
    Write-Info "Checking npm..."

    $ver = $null
    try { $ver = & npm --version 2>$null } catch { }

    if (-not $ver) {
        Stop-WithError "npm is not available." @(
            "npm is bundled with Node.js.",
            "Re-install Node from: https://nodejs.org"
        )
    }

    Write-Ok "npm $ver"
}

function Test-UiDependencies {
    Write-Info "Checking UI packages..."

    $nodeModules = Join-Path $UiDir "node_modules"
    if (Test-Path $nodeModules) {
        Write-Ok "UI packages already installed"
        return
    }

    Write-Warn "UI packages not installed. Running npm install..."
    Push-Location $UiDir
    try {
        & npm install --silent
    } catch { }
    $exitCode = $LASTEXITCODE
    Pop-Location

    if ($exitCode -ne 0) {
        Stop-WithError "npm install failed for the UI." @(
            "Try running it manually: cd src\ui && npm install",
            "Check your internet connection and npm registry access."
        )
    }

    Write-Ok "UI packages installed"
}

# ── Database migration ─────────────────────────────────────────────────────────

function Invoke-Migrations {
    Write-Info "Applying database migrations..."

    Push-Location $ApiDir
    try {
        & dotnet ef database update --project $InfraProject --startup-project $StartupProj 2>&1 | Out-Null
    } catch { }
    $exitCode = $LASTEXITCODE
    Pop-Location

    if ($exitCode -ne 0) {
        Stop-WithError "Database migration failed." @(
            "Check the connection string in: src\api\TaskTracker.Api\appsettings.Development.json",
            "Make sure the folder for the SQLite file is writable."
        )
    }

    Write-Ok "Database is up to date"
}

# ── API startup (new window) ───────────────────────────────────────────────────

function Start-ApiWindow {
    Write-Info "Launching API in a new terminal window..."

    $windowTitle = 'OCAS Tracker - API'
    $apiCmd = "
`$Host.UI.RawUI.WindowTitle = '$windowTitle'
Set-Location '$ApiProject'
`$env:ASPNETCORE_ENVIRONMENT = 'Development'
Write-Host ''
Write-Host '  OCAS Tracker - API' -ForegroundColor Cyan
Write-Host '  URL     : $ApiUrl' -ForegroundColor DarkGray
Write-Host '  Swagger : $SwaggerUrl' -ForegroundColor DarkGray
Write-Host '  Press Ctrl+C to stop.' -ForegroundColor DarkGray
Write-Host ''
dotnet run --launch-profile http
if (`$LASTEXITCODE -ne 0) {
    Write-Host ''
    Write-Host '  [X] API exited with an error.' -ForegroundColor Red
    Write-Host '      Common causes: port 5200 already in use, or a build error.' -ForegroundColor DarkGray
    Write-Host '      Run: netstat -ano | findstr :5200' -ForegroundColor DarkGray
}
Write-Host ''
Write-Host '  Press any key to close...' -ForegroundColor DarkGray
`$null = `$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
"

    try {
        Start-Process powershell -ArgumentList "-NoExit", "-Command", $apiCmd
    } catch {
        Stop-WithError "Failed to open a new terminal window for the API." @(
            "Start the API manually in a separate terminal:",
            "  cd src\api\TaskTracker.Api",
            "  `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"
        )
    }

    Write-Ok "API window opened"
    Write-Hint "API     : $ApiUrl"
    Write-Hint "Swagger : $SwaggerUrl"
}

# ── UI startup (current window) ────────────────────────────────────────────────

function Start-Ui {
    Write-Host ""
    Write-Info "Starting UI dev server..."
    Write-Hint "URL : $UiUrl"
    Write-Hint "Press Ctrl+C to stop."
    Write-Host ""

    Start-Sleep -Seconds 2

    Push-Location $UiDir
    try {
        & npm run dev
    } catch { }
    $exitCode = $LASTEXITCODE
    Pop-Location

    if ($exitCode -ne 0) {
        Stop-WithError "The UI dev server exited with an error." @(
            "Check whether port 5173 is already in use:",
            "  netstat -ano | findstr :5173",
            "Try manually: cd src\ui && npm run dev"
        )
    }
}

# ── Main ───────────────────────────────────────────────────────────────────────

Write-Banner

Write-Host "  Checking prerequisites..." -ForegroundColor DarkGray
Write-Host ""
Test-DotNet
Test-EfTool
Test-Node
Test-Npm
Test-UiDependencies

Write-Host ""
Write-Host "  Setting up..." -ForegroundColor DarkGray
Write-Host ""
Invoke-Migrations
Start-ApiWindow

Write-Host ""
Write-Host "  --------------------------------------------" -ForegroundColor DarkGray
Write-Host "  API     : $ApiUrl  (separate window)"        -ForegroundColor White
Write-Host "  Swagger : $SwaggerUrl"                       -ForegroundColor DarkGray
Write-Host "  UI      : $UiUrl  (this window)"             -ForegroundColor White
Write-Host "  --------------------------------------------" -ForegroundColor DarkGray

Start-Ui
