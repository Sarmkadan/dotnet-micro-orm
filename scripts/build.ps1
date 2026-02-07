# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

param(
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$SkipPack
)

$ErrorActionPreference = "Stop"

# Colors
$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"

Write-Host "=== DotnetMicroOrm Build Script ===" -ForegroundColor $Green
Write-Host ""

# Check .NET installation
try {
    $version = dotnet --version
    Write-Host "Current .NET version: $version"
}
catch {
    Write-Host "Error: .NET SDK is not installed" -ForegroundColor $Red
    exit 1
}

Write-Host ""

# Clean
Write-Host "Cleaning previous builds..." -ForegroundColor $Yellow
dotnet clean --configuration $Configuration 2> $null
Remove-Item -Path "bin\$Configuration\net10.0" -Recurse -Force 2> $null

# Restore
Write-Host "Restoring dependencies..." -ForegroundColor $Yellow
dotnet restore

# Build
Write-Host "Building project..." -ForegroundColor $Yellow
dotnet build --configuration $Configuration --no-restore

# Tests
if (-not $SkipTests) {
    Write-Host "Running tests..." -ForegroundColor $Yellow
    dotnet test --configuration $Configuration --no-build --verbosity minimal
}

# Pack
if (-not $SkipPack) {
    Write-Host "Creating NuGet package..." -ForegroundColor $Yellow
    dotnet pack --configuration $Configuration --output ./nupkg --no-build
}

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor $Green
Write-Host "Configuration: $Configuration"
Write-Host "Output directory: bin\$Configuration\net10.0"

if (-not $SkipPack) {
    Write-Host "NuGet package: ./nupkg/"
}

exit 0
