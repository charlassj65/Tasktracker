#Requires -Version 5.1
<#
.SYNOPSIS
    Starts the OCAS Tracker API in Development mode.
.DESCRIPTION
    Checks all prerequisites, applies any pending database migrations,
    then starts the ASP.NET Core API on http://localhost:5200.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ApiDir       = Join-Path $PSScriptRoot "src\api"
$ApiProject   = Join-Path $ApiDir "TaskTracker.Api"
$InfraProject = "TaskTracker.Infrastructure"
$StartupProj  = "TaskTracker.Api"
$ApiUrl       = "http://localhost:5200"
$SwaggerUrl   = "$ApiUrl/swagger"

# ── Helpers ────────────────────────────────────────────────────────────────────

function Write-Banner {
    Write-Host ""
    Write-Host "  ============================================" -ForegroundColor Cyan
    Write-Host "       OCAS Tracker  -  API Start            " -ForegroundColor Cyan
    Write-Host "  ============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Ok($msg)   { Write-Host "  [OK] $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "  [X]  $msg" -ForegroundColor Red }
function Write-Info($msg) { Write-Host "  [>]  $msg" -ForegroundColor Cyan }
function Write-Hint($msg) { Write-Host "       $msg" -ForegroundColor DarkGray }

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

# ── Database migration ─────────────────────────────────────────────────────────

function Invoke-Migrations {
    Write-Info "Applying database migrations..."

    Push-Location $ApiDir
    $migResult = $null
    try {
        $migResult = & dotnet ef database update --project $InfraProject --startup-project $StartupProj 2>&1
    } catch { }
    Pop-Location

    if ($LASTEXITCODE -ne 0) {
        Stop-WithError "Database migration failed." @(
            "Check the connection string in: src\api\TaskTracker.Api\appsettings.Development.json",
            "Make sure the folder for the SQLite file is writable."
        )
    }

    Write-Ok "Database is up to date"
}

# ── API startup ────────────────────────────────────────────────────────────────

function Start-Api {
    Write-Info "Starting API in Development mode..."
    Write-Hint "URL     : $ApiUrl"
    Write-Hint "Swagger : $SwaggerUrl"
    Write-Hint "Press Ctrl+C to stop."
    Write-Host ""

    Push-Location $ApiProject
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    try {
        & dotnet run --launch-profile http
    } catch { }
    $exitCode = $LASTEXITCODE
    $env:ASPNETCORE_ENVIRONMENT = $null
    Pop-Location

    if ($exitCode -ne 0) {
        Stop-WithError "The API exited with an error." @(
            "Check whether port 5200 is already in use:",
            "  netstat -ano | findstr :5200",
            "Review the log output above for details."
        )
    }
}

# ── Main ───────────────────────────────────────────────────────────────────────

Write-Banner
Test-DotNet
Test-EfTool
Invoke-Migrations
Start-Api
