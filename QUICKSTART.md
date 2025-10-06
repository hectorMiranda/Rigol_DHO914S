# Quick Start Guide

Welcome to the Rigol DHO914S Oscilloscope Tools! This guide will get you up and running in minutes.

## 🚀 Fastest Way to Start

### Windows Users
1. **Double-click `run_tools.bat`** - This will handle everything automatically!
2. Choose option 1 to take your first screenshot
3. Choose option 2 to see your oscilloscope information

### Command Line Users
```bash
# Install dependencies
pip install -r requirements.txt

# Take a screenshot
python scripts/screenshot.py --output my_first_screenshot.png

# Get device info
python scripts/scope_info.py
```

### PowerShell Users
```powershell
# Use the interactive PowerShell script
.\rigol_tools.ps1

# Or run specific commands
.\rigol_tools.ps1 screenshot -Timestamp
.\rigol_tools.ps1 info -Verbose
.\rigol_tools.ps1 export -AllChannels -Output data
```

## 📋 Prerequisites

1. **Rigol DHO914S Oscilloscope** - Connected via USB or Ethernet
2. **Python 3.7+** - Download from [python.org](https://python.org)
3. **USB Drivers** - Install Rigol drivers if using USB connection

## 🔌 Connection Methods

### USB Connection (Recommended)
- Connect oscilloscope via USB cable
- Install Rigol USB drivers
- Tools will auto-detect the device

### Ethernet Connection
- Configure oscilloscope with IP address
- Use `--ethernet IP_ADDRESS` option
- Example: `python scripts/screenshot.py --ethernet 192.168.1.100`

## 🎯 Common Tasks

### Take Screenshots
```bash
# Simple screenshot
python scripts/screenshot.py

# With timestamp and JPEG format
python scripts/screenshot.py --timestamp --format JPEG

# Multiple screenshots
python scripts/screenshot.py --multiple 5 --interval 2.0
```

### Export Waveform Data
```bash
# Export all channels to CSV and plots
python scripts/waveform_export.py --all-channels --format csv,plot --output data/

# Export specific channels
python scripts/waveform_export.py --channels 1,2 --format csv --output exports/
```

### Get Device Information
```bash
# Basic info
python scripts/scope_info.py

# Detailed info with measurements
python scripts/scope_info.py --verbose --measurements 1,2,3,4
```

### Run Examples
```bash
# Basic usage examples
python src/examples/basic_usage.py

# Advanced waveform analysis
python src/examples/waveform_capture.py

# Screenshot tool demonstration
python src/examples/screenshot_tool.py
```

## 📝 Programming Examples

### Simple Python Script
```python
from rigol_dho914s import RigolDHO914S

# Connect and get info
with RigolDHO914S() as scope:
    print("Connected to:", scope.get_identity())
    
    # Take a screenshot
    scope.take_screenshot('my_scope.png')
    
    # Configure Channel 1
    scope.set_channel_enable(1, True)
    scope.set_channel_scale(1, 0.5)  # 500mV/div
    scope.set_channel_coupling(1, 'DC')
    
    # Capture waveform
    waveform = scope.get_waveform_data(1)
    print(f"Captured {len(waveform['voltage'])} data points")
    
    # Save to CSV
    scope.save_waveform_csv(waveform, 'waveform.csv', 1)
```

### Ethernet Connection
```python
from rigol_dho914s import RigolDHO914S

# Connect via Ethernet
with RigolDHO914S(connection_type='ethernet', ip_address='192.168.1.100') as scope:
    scope.take_screenshot('network_screenshot.png')
```

## 🛠️ Installation as Package

For advanced users who want to install globally:

```bash
# Install in development mode
pip install -e .

# Now you can use these commands anywhere:
rigol-screenshot --output test.png
rigol-info --verbose
rigol-export --all-channels --output data/
```

## 🧪 Testing Your Setup

Run the automated test suite to verify everything works:

```bash
python src/examples/automated_test.py --output test_results
```

This will:
- Test connection
- Take screenshots
- Test all channel configurations
- Capture waveforms
- Run measurements
- Generate a comprehensive report

## ❌ Troubleshooting

### "No Rigol USB devices found"
- Check USB cable connection
- Install Rigol USB drivers
- Try a different USB port

### "PyVISA not found"
```bash
pip install pyvisa pyvisa-py
```

### "Connection timeout"
- Verify oscilloscope is powered on
- Check IP address for Ethernet connections
- Try increasing timeout: add `--timeout 10` to commands

### Permission Errors
- Run as Administrator (Windows)
- Check antivirus software

## 📚 More Information

- **API Reference**: `docs/api_reference.md`
- **Troubleshooting**: `docs/troubleshooting.md`
- **Examples**: `src/examples/` directory
- **GitHub Repository**: https://github.com/hectorMiranda/Rigol_DHO914S

## 🎉 You're Ready!

Start by running `run_tools.bat` (Windows) or trying the basic examples. The tools are designed to be intuitive and will guide you through any issues.

Happy oscilloscope controlling! 🔬📊
