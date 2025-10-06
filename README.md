# Rigol DHO914S Oscilloscope Control Tools

A comprehensive Python toolkit for controlling and automating the Rigol DHO914S oscilloscope via SCPI commands over USB/Ethernet.

## Features

- 📸 **Screenshot Capture**: Take and save oscilloscope screenshots
- 🎛️ **Channel Control**: Configure channels, voltage scales, and coupling
- 📊 **Waveform Acquisition**: Capture and analyze waveform data
- ⚙️ **Trigger Setup**: Configure various trigger modes and parameters
- 🔧 **Measurement Tools**: Automated measurements and cursors
- 💾 **Data Export**: Save waveforms in various formats (CSV, binary)
- 🖥️ **Remote Control**: Full remote operation capabilities

## Requirements

- Python 3.7+
- Rigol DHO914S Oscilloscope
- USB cable or Ethernet connection
- Required Python packages (see `requirements.txt`)

## Installation

1. Clone this repository:
```bash
git clone https://github.com/hectorMiranda/Rigol_DHO914S.git
cd Rigol_DHO914S
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Connect your oscilloscope via USB or configure Ethernet connection

## Quick Start

### Basic Connection Test
```python
from rigol_dho914s import RigolDHO914S

# Connect via USB (auto-detect)
scope = RigolDHO914S()

# Or connect via Ethernet
# scope = RigolDHO914S(connection_type='ethernet', ip_address='192.168.1.100')

# Test connection
print(scope.get_identity())
```

### Take a Screenshot
```python
scope.take_screenshot('outputs/screenshot.png')
```

### Configure Channel and Capture Waveform
```python
# Setup channel 1
scope.set_channel_scale(1, 0.5)  # 500mV/div
scope.set_channel_coupling(1, 'DC')
scope.set_channel_enable(1, True)

# Capture waveform data
waveform = scope.get_waveform_data(1)
scope.save_waveform_csv(waveform, 'outputs/channel1_data.csv')
```

## Project Structure

```
Rigol_DHO914S/
├── src/
│   ├── rigol_dho914s/
│   │   ├── __init__.py
│   │   ├── core.py              # Main oscilloscope class
│   │   ├── commands.py          # SCPI command definitions
│   │   ├── utils.py             # Utility functions
│   │   └── exceptions.py        # Custom exceptions
│   └── examples/
│       ├── basic_usage.py       # Basic usage examples
│       ├── screenshot_tool.py   # Screenshot utility
│       ├── waveform_capture.py  # Waveform acquisition
│       └── automated_test.py    # Automated test suite
├── outputs/                     # Generated files (screenshots, data)
├── tests/
│   ├── test_connection.py
│   ├── test_commands.py
│   └── test_waveforms.py
├── docs/
│   ├── api_reference.md
│   ├── scpi_commands.md
│   └── troubleshooting.md
├── scripts/
│   ├── screenshot.py            # CLI screenshot tool
│   ├── scope_info.py           # Device information
│   └── waveform_export.py      # Batch waveform export
├── requirements.txt
├── setup.py
├── .gitignore
└── README.md
```

## Command Line Tools

### Screenshot Tool
```bash
python scripts/screenshot.py --output my_screenshot.png --format PNG
```

### Scope Information
```bash
python scripts/scope_info.py
```

### Waveform Export
```bash
python scripts/waveform_export.py --channels 1,2 --format CSV
```

*Note: All output files are saved to the `outputs/` directory by default.*

## API Reference

### Core Functions

- `RigolDHO914S()` - Initialize connection
- `take_screenshot(filename)` - Capture screenshot
- `get_waveform_data(channel)` - Get waveform data
- `set_channel_scale(channel, scale)` - Set voltage scale
- `set_timebase(scale)` - Set time scale
- `set_trigger_source(source)` - Configure trigger
- `run()` / `stop()` / `single()` - Acquisition control

### Supported Connection Types

- **USB**: Automatic device detection
- **Ethernet**: TCP/IP connection (requires IP address)

## Examples

See the `src/examples/` directory for comprehensive usage examples.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

MIT License - see LICENSE file for details

## Troubleshooting

- **Connection Issues**: Check USB drivers or network connectivity
- **Permission Errors**: Run as administrator (Windows) or with sudo (Linux)
- **Timeout Errors**: Increase timeout values in connection settings

For more detailed troubleshooting, see `docs/troubleshooting.md`

## Support

- 📧 Open an issue for bug reports
- 💡 Feature requests welcome
- 📚 Check documentation in `docs/` folder

---

**Note**: This tool is not affiliated with Rigol Technologies. Use at your own risk and ensure compliance with your equipment's warranty terms.
