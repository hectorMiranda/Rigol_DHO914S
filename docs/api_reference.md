# Rigol DHO914S API Reference

This document provides a comprehensive reference for the Rigol DHO914S Python library.

## Core Classes

### RigolDHO914S

The main class for controlling the oscilloscope.

#### Constructor

```python
RigolDHO914S(connection_type='usb', resource_string=None, ip_address=None, timeout=5000)
```

**Parameters:**
- `connection_type` (str): 'usb' or 'ethernet'
- `resource_string` (str, optional): Specific VISA resource string
- `ip_address` (str, optional): IP address for Ethernet connection
- `timeout` (float): Command timeout in milliseconds

#### Connection Methods

##### `close()`
Close the connection to the oscilloscope.

##### `get_identity()`
Get the oscilloscope identity string.

**Returns:** str - Device identity

#### System Control Methods

##### `reset()`
Reset the oscilloscope to default settings.

##### `clear_status()`
Clear the oscilloscope status registers.

##### `get_error()`
Get the last error from the oscilloscope.

**Returns:** str - Error message

#### Acquisition Control Methods

##### `run()`
Start continuous acquisition.

##### `stop()`
Stop acquisition.

##### `single()`
Set single trigger mode.

##### `force_trigger()`
Force a trigger event.

#### Channel Control Methods

##### `set_channel_enable(channel, enable)`
Enable or disable a channel.

**Parameters:**
- `channel` (int): Channel number (1-4)
- `enable` (bool): True to enable, False to disable

##### `set_channel_coupling(channel, coupling)`
Set channel input coupling.

**Parameters:**
- `channel` (int): Channel number (1-4)
- `coupling` (str): 'AC', 'DC', or 'GND'

##### `set_channel_scale(channel, scale)`
Set channel vertical scale.

**Parameters:**
- `channel` (int): Channel number (1-4)
- `scale` (float): Volts per division

##### `set_channel_offset(channel, offset)`
Set channel vertical offset.

**Parameters:**
- `channel` (int): Channel number (1-4)
- `offset` (float): Offset in volts

##### `set_channel_probe(channel, ratio)`
Set channel probe ratio.

**Parameters:**
- `channel` (int): Channel number (1-4)
- `ratio` (float): Probe ratio (e.g., 1.0, 10.0)

##### `get_channel_scale(channel)`
Get channel vertical scale.

**Parameters:**
- `channel` (int): Channel number (1-4)

**Returns:** float - Volts per division

##### `get_channel_offset(channel)`
Get channel vertical offset.

**Parameters:**
- `channel` (int): Channel number (1-4)

**Returns:** float - Offset in volts

##### `get_channel_coupling(channel)`
Get channel input coupling.

**Parameters:**
- `channel` (int): Channel number (1-4)

**Returns:** str - Coupling setting

#### Timebase Control Methods

##### `set_timebase_scale(scale)`
Set horizontal timebase scale.

**Parameters:**
- `scale` (float): Seconds per division

##### `set_timebase_offset(offset)`
Set horizontal timebase offset.

**Parameters:**
- `offset` (float): Offset in seconds

##### `get_timebase_scale()`
Get horizontal timebase scale.

**Returns:** float - Seconds per division

##### `get_timebase_offset()`
Get horizontal timebase offset.

**Returns:** float - Offset in seconds

#### Trigger Control Methods

##### `set_trigger_source(source)`
Set trigger source.

**Parameters:**
- `source` (str): Trigger source ('CHAN1', 'CHAN2', 'CHAN3', 'CHAN4', 'EXT', etc.)

##### `set_trigger_level(level)`
Set trigger level.

**Parameters:**
- `level` (float): Trigger level in volts

##### `set_trigger_slope(slope)`
Set trigger slope.

**Parameters:**
- `slope` (str): 'POS', 'NEG', or 'RFAL'

##### `get_trigger_status()`
Get current trigger status.

**Returns:** str - Trigger status

#### Screenshot Methods

##### `take_screenshot(filename, format='PNG')`
Take a screenshot of the oscilloscope display.

**Parameters:**
- `filename` (str): Output filename
- `format` (str): Image format ('PNG', 'BMP', 'JPEG')

#### Waveform Acquisition Methods

##### `get_waveform_data(channel, points=None)`
Get waveform data from specified channel.

**Parameters:**
- `channel` (int): Channel number (1-4)
- `points` (int, optional): Number of points to acquire

**Returns:** dict - Dictionary with 'time', 'voltage', and 'preamble' keys

##### `save_waveform_csv(waveform_data, filename, channel=1)`
Save waveform data to CSV file.

**Parameters:**
- `waveform_data` (dict): Waveform data from `get_waveform_data()`
- `filename` (str): Output filename
- `channel` (int): Channel number for header

#### Measurement Methods

##### `measure(measurement_type, channel)`
Perform automatic measurement.

**Parameters:**
- `measurement_type` (str): Type of measurement (see MeasurementTypes)
- `channel` (int): Channel number (1-4)

**Returns:** float - Measurement value

##### `get_voltage_measurements(channel)`
Get common voltage measurements for a channel.

**Parameters:**
- `channel` (int): Channel number (1-4)

**Returns:** dict - Dictionary of voltage measurements

##### `get_time_measurements(channel)`
Get common time measurements for a channel.

**Parameters:**
- `channel` (int): Channel number (1-4)

**Returns:** dict - Dictionary of time measurements

#### Utility Methods

##### `wait_for_operation_complete(timeout=10.0)`
Wait for the oscilloscope to complete current operation.

**Parameters:**
- `timeout` (float): Timeout in seconds

##### `get_system_status()`
Get comprehensive system status information.

**Returns:** dict - System status dictionary

## Constants and Enums

### MeasurementTypes

Available measurement types for automatic measurements:

#### Voltage Measurements
- `VOLTAGE_MAX` - Maximum voltage
- `VOLTAGE_MIN` - Minimum voltage
- `VOLTAGE_PP` - Peak-to-peak voltage
- `VOLTAGE_TOP` - Top voltage
- `VOLTAGE_BASE` - Base voltage
- `VOLTAGE_AMP` - Amplitude
- `VOLTAGE_HIGH` - High voltage
- `VOLTAGE_LOW` - Low voltage
- `VOLTAGE_AVERAGE` - Average voltage
- `VOLTAGE_RMS` - RMS voltage
- `VOLTAGE_OVERSHOOT` - Overshoot
- `VOLTAGE_PRESHOOT` - Preshoot

#### Time Measurements
- `PERIOD` - Period
- `FREQUENCY` - Frequency
- `RISE_TIME` - Rise time
- `FALL_TIME` - Fall time
- `PULSE_WIDTH_POSITIVE` - Positive pulse width
- `PULSE_WIDTH_NEGATIVE` - Negative pulse width
- `DUTY_CYCLE_POSITIVE` - Positive duty cycle
- `DUTY_CYCLE_NEGATIVE` - Negative duty cycle

#### Delay Measurements
- `DELAY_RISING_RISING` - Rising edge to rising edge delay
- `DELAY_FALLING_FALLING` - Falling edge to falling edge delay
- `DELAY_RISING_FALLING` - Rising edge to falling edge delay
- `DELAY_FALLING_RISING` - Falling edge to rising edge delay

## Exception Classes

### RigolError
Base exception for all oscilloscope-related errors.

### ConnectionError
Raised when there's an issue connecting to the oscilloscope.

### CommandError
Raised when a SCPI command fails or returns an error.

### TimeoutError
Raised when a command times out.

### DataError
Raised when there's an issue with data transfer or parsing.

## Usage Examples

### Basic Connection
```python
from rigol_dho914s import RigolDHO914S

# USB connection
scope = RigolDHO914S()

# Ethernet connection
scope = RigolDHO914S(connection_type='ethernet', ip_address='192.168.1.100')

# Using context manager (recommended)
with RigolDHO914S() as scope:
    print(scope.get_identity())
```

### Channel Configuration
```python
with RigolDHO914S() as scope:
    # Configure Channel 1
    scope.set_channel_enable(1, True)
    scope.set_channel_coupling(1, 'DC')
    scope.set_channel_scale(1, 0.5)  # 500mV/div
    scope.set_channel_offset(1, 0.0)
    scope.set_channel_probe(1, 1.0)  # 1x probe
```

### Taking Screenshots
```python
with RigolDHO914S() as scope:
    scope.take_screenshot('screenshot.png', format='PNG')
```

### Waveform Capture
```python
with RigolDHO914S() as scope:
    # Capture waveform from Channel 1
    waveform = scope.get_waveform_data(1)
    
    # Save to CSV
    scope.save_waveform_csv(waveform, 'channel1_data.csv', 1)
    
    # Access data arrays
    time_data = waveform['time']
    voltage_data = waveform['voltage']
```

### Measurements
```python
with RigolDHO914S() as scope:
    # Single measurement
    frequency = scope.measure('FREQ', 1)
    
    # Multiple measurements
    voltage_measurements = scope.get_voltage_measurements(1)
    time_measurements = scope.get_time_measurements(1)
```

### Trigger Configuration
```python
with RigolDHO914S() as scope:
    scope.set_trigger_source('CHAN1')
    scope.set_trigger_level(0.0)
    scope.set_trigger_slope('POS')
```

### Timebase Configuration
```python
with RigolDHO914S() as scope:
    scope.set_timebase_scale(1e-3)  # 1ms/div
    scope.set_timebase_offset(0.0)
```

## Error Handling

```python
from rigol_dho914s import RigolDHO914S, ConnectionError, CommandError

try:
    with RigolDHO914S() as scope:
        # Your code here
        pass
except ConnectionError as e:
    print(f"Could not connect to oscilloscope: {e}")
except CommandError as e:
    print(f"Command failed: {e}")
except Exception as e:
    print(f"Unexpected error: {e}")
```
