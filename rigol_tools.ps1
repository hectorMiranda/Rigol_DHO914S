# Rigol DHO914S Oscilloscope Tools - PowerShell Script
# This script provides easy access to common oscilloscope operations

param(
    [Parameter(Position=0)]
    [ValidateSet('screenshot', 'info', 'export', 'examples', 'test', 'install', 'help')]
    [string]$Command,
    
    [string]$IP,
    [string]$Output = ".",
    [string]$Channels = "1,2",
    [string]$Format = "PNG",
    [switch]$Verbose,
    [switch]$AllChannels,
    [switch]$Timestamp
)

function Write-Banner {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " Rigol DHO914S Oscilloscope Tools" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Test-Dependencies {
    # Check Python
    try {
        $pythonVersion = python --version 2>&1
        Write-Host "✓ Python found: $pythonVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Python not found. Please install Python 3.7+ and add to PATH" -ForegroundColor Red
        exit 1
    }
    
    # Check PyVISA
    try {
        python -c "import pyvisa" 2>$null
        Write-Host "✓ PyVISA available" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠ Installing required packages..." -ForegroundColor Yellow
        pip install -r requirements.txt
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Failed to install required packages" -ForegroundColor Red
            exit 1
        }
    }
}

function Invoke-Screenshot {
    Write-Host "📸 Taking screenshot..." -ForegroundColor Blue
    
    $args = @("scripts\screenshot.py")
    
    if ($IP) { $args += @("--ethernet", $IP) }
    if ($Timestamp) { $args += "--timestamp" }
    if ($Format) { $args += @("--format", $Format) }
    if ($Verbose) { $args += "--verbose" }
    
    python @args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Screenshot completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Screenshot failed. Check connection and try again." -ForegroundColor Red
    }
}

function Get-DeviceInfo {
    Write-Host "📋 Getting device information..." -ForegroundColor Blue
    
    $args = @("scripts\scope_info.py")
    
    if ($IP) { $args += @("--ethernet", $IP) }
    if ($Verbose) { $args += "--verbose" }
    
    python @args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Device information retrieved successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed to get device info. Check connection and try again." -ForegroundColor Red
    }
}

function Export-Waveforms {
    Write-Host "📊 Exporting waveform data..." -ForegroundColor Blue
    
    # Create output directory if it doesn't exist
    if (!(Test-Path $Output)) {
        New-Item -ItemType Directory -Path $Output -Force | Out-Null
        Write-Host "📁 Created output directory: $Output" -ForegroundColor Yellow
    }
    
    $args = @("scripts\waveform_export.py")
    
    if ($AllChannels) {
        $args += "--all-channels"
    } else {
        $args += @("--channels", $Channels)
    }
    
    $args += @("--format", "csv,plot")
    $args += @("--output", $Output)
    
    if ($IP) { $args += @("--ethernet", $IP) }
    if ($Verbose) { $args += "--verbose" }
    
    python @args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Waveform export completed successfully!" -ForegroundColor Green
        Write-Host "📁 Data saved to: $Output" -ForegroundColor Yellow
    } else {
        Write-Host "✗ Waveform export failed. Check connection and try again." -ForegroundColor Red
    }
}

function Invoke-Examples {
    Write-Host "🚀 Running basic usage examples..." -ForegroundColor Blue
    
    python src\examples\basic_usage.py
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Examples completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Examples failed. Check connection and try again." -ForegroundColor Red
    }
}

function Invoke-TestSuite {
    Write-Host "🧪 Running automated test suite..." -ForegroundColor Blue
    
    $testOutput = "test_results"
    if (!(Test-Path $testOutput)) {
        New-Item -ItemType Directory -Path $testOutput -Force | Out-Null
    }
    
    $args = @("src\examples\automated_test.py", "--output", $testOutput)
    
    if ($IP) { $args += @("--ethernet", $IP) }
    if ($Verbose) { $args += "--verbose" }
    
    python @args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Test suite completed successfully!" -ForegroundColor Green
        Write-Host "📁 Results saved to: $testOutput" -ForegroundColor Yellow
    } else {
        Write-Host "✗ Test suite failed. Check connection and try again." -ForegroundColor Red
    }
}

function Install-Package {
    Write-Host "📦 Installing package in development mode..." -ForegroundColor Blue
    
    pip install -e .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Package installed successfully!" -ForegroundColor Green
        Write-Host "🎯 You can now use these commands:" -ForegroundColor Yellow
        Write-Host "   - rigol-screenshot" -ForegroundColor Cyan
        Write-Host "   - rigol-info" -ForegroundColor Cyan
        Write-Host "   - rigol-export" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Installation failed." -ForegroundColor Red
    }
}

function Show-Help {
    Write-Host "Usage: .\rigol_tools.ps1 <command> [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Cyan
    Write-Host "  screenshot    Take a screenshot" -ForegroundColor White
    Write-Host "  info          Get device information" -ForegroundColor White
    Write-Host "  export        Export waveform data" -ForegroundColor White
    Write-Host "  examples      Run basic usage examples" -ForegroundColor White
    Write-Host "  test          Run automated test suite" -ForegroundColor White
    Write-Host "  install       Install package in development mode" -ForegroundColor White
    Write-Host "  help          Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Cyan
    Write-Host "  -IP <address>     Use Ethernet connection" -ForegroundColor White
    Write-Host "  -Output <path>    Output directory (default: current)" -ForegroundColor White
    Write-Host "  -Channels <list>  Channels to export (default: 1,2)" -ForegroundColor White
    Write-Host "  -Format <format>  Screenshot format (default: PNG)" -ForegroundColor White
    Write-Host "  -AllChannels      Export all 4 channels" -ForegroundColor White
    Write-Host "  -Timestamp        Add timestamp to files" -ForegroundColor White
    Write-Host "  -Verbose          Enable verbose output" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\rigol_tools.ps1 screenshot -Timestamp -Format JPEG" -ForegroundColor Gray
    Write-Host "  .\rigol_tools.ps1 info -IP 192.168.1.100 -Verbose" -ForegroundColor Gray
    Write-Host "  .\rigol_tools.ps1 export -AllChannels -Output data" -ForegroundColor Gray
    Write-Host "  .\rigol_tools.ps1 test -IP 192.168.1.100" -ForegroundColor Gray
}

function Show-Menu {
    Write-Host "Please select an option:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Take a screenshot" -ForegroundColor White
    Write-Host "2. Get device information" -ForegroundColor White
    Write-Host "3. Export waveform data" -ForegroundColor White
    Write-Host "4. Run basic usage examples" -ForegroundColor White
    Write-Host "5. Run automated test suite" -ForegroundColor White
    Write-Host "6. Install as package" -ForegroundColor White
    Write-Host "7. Exit" -ForegroundColor White
    Write-Host ""
    
    $choice = Read-Host "Enter your choice (1-7)"
    
    switch ($choice) {
        "1" { Invoke-Screenshot }
        "2" { Get-DeviceInfo }
        "3" { Export-Waveforms }
        "4" { Invoke-Examples }
        "5" { Invoke-TestSuite }
        "6" { Install-Package }
        "7" { exit 0 }
        default { 
            Write-Host "Invalid choice. Please try again." -ForegroundColor Red
            Show-Menu
        }
    }
    
    Write-Host ""
    Read-Host "Press Enter to continue"
    Show-Menu
}

# Main script execution
Write-Banner

# Test dependencies
Test-Dependencies

# Execute based on command line arguments
if ($Command) {
    switch ($Command.ToLower()) {
        "screenshot" { Invoke-Screenshot }
        "info" { Get-DeviceInfo }
        "export" { Export-Waveforms }
        "examples" { Invoke-Examples }
        "test" { Invoke-TestSuite }
        "install" { Install-Package }
        "help" { Show-Help }
        default { 
            Write-Host "Unknown command: $Command" -ForegroundColor Red
            Show-Help
        }
    }
} else {
    # Show interactive menu
    Show-Menu
}
