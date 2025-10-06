# Troubleshooting Guide

This guide helps resolve common issues when using the Rigol DHO914S Python library.

## Connection Issues

### USB Connection Problems

#### Issue: "No Rigol USB devices found"

**Possible Causes:**
- Oscilloscope is not connected
- USB drivers not installed
- Oscilloscope is turned off
- USB cable is faulty

**Solutions:**
1. Verify the oscilloscope is powered on
2. Check USB cable connection
3. Install Rigol USB drivers from the official website
4. Try a different USB cable
5. Test with different USB port
6. Check Windows Device Manager for USB devices

#### Issue: "PyVISA not available" or ImportError

**Solution:**
```bash
pip install pyvisa pyvisa-py
```

For NI-VISA backend (recommended for better compatibility):
1. Download and install NI-VISA from National Instruments website
2. Install PyVISA: `pip install pyvisa`

#### Issue: "Access denied" or permission errors

**Solutions:**
- Run Python script as Administrator (Windows)
- Add user to appropriate groups for device access (Linux)
- Check antivirus software blocking device access

### Ethernet Connection Problems

#### Issue: "Connection timeout" or "Cannot connect to IP address"

**Possible Causes:**
- Incorrect IP address
- Network connectivity issues
- Firewall blocking connection
- Oscilloscope Ethernet not configured

**Solutions:**
1. Verify oscilloscope IP address in system menu
2. Ping the oscilloscope: `ping 192.168.1.100`
3. Check network cable connection
4. Verify both devices are on same network
5. Disable firewall temporarily for testing
6. Check oscilloscope Ethernet settings

#### Issue: "Slow response" or intermittent disconnections

**Solutions:**
1. Increase timeout value in connection
2. Check network traffic and bandwidth
3. Use direct Ethernet connection (no switches/routers)
4. Verify network cable quality

## Command and Communication Issues

### SCPI Command Errors

#### Issue: "Command not recognized" or syntax errors

**Solutions:**
1. Check SCPI command syntax in oscilloscope manual
2. Verify firmware version compatibility
3. Use correct command format (case sensitive)
4. Check for trailing spaces or newlines

#### Issue: "Device returned error" messages

**Solutions:**
1. Check oscilloscope error queue: `scope.get_error()`
2. Clear error status: `scope.clear_status()`
3. Verify command parameters are valid
4. Check measurement ranges and limits

### Timeout Issues

#### Issue: Commands timing out

**Solutions:**
1. Increase timeout value:
   ```python
   scope = RigolDHO914S(timeout=10000)  # 10 seconds
   ```
2. Wait for operations to complete:
   ```python
   scope.wait_for_operation_complete()
   ```
3. Check if oscilloscope is busy with long operations

## Data Acquisition Issues

### Waveform Capture Problems

#### Issue: "No waveform data" or empty arrays

**Possible Causes:**
- Channel not enabled
- No signal connected
- Trigger not configured properly
- Acquisition not running

**Solutions:**
1. Enable the channel: `scope.set_channel_enable(1, True)`
2. Start acquisition: `scope.run()`
3. Check trigger settings
4. Verify signal is connected and within range

#### Issue: "Waveform data appears truncated"

**Solutions:**
1. Check memory depth settings
2. Adjust timebase scale for desired time span
3. Use appropriate sample rate for signal frequency

#### Issue: "Incorrect voltage scaling"

**Solutions:**
1. Verify probe ratio settings: `scope.set_channel_probe(1, 10.0)`
2. Check channel scale and offset settings
3. Calibrate probes if necessary

### Screenshot Issues

#### Issue: "Screenshot file is corrupted" or empty

**Solutions:**
1. Check supported image formats (PNG, BMP, JPEG)
2. Verify output directory exists and is writable
3. Try different image format
4. Check available disk space

#### Issue: "Screenshot is blank" or shows wrong content

**Solutions:**
1. Ensure oscilloscope display is on
2. Wait for display to update before taking screenshot
3. Check if remote control is interfering with display

## Performance Issues

### Slow Operation

#### Issue: Commands execute slowly

**Solutions:**
1. Use USB connection instead of Ethernet when possible
2. Reduce number of queries and commands
3. Batch operations when possible
4. Close other applications using VISA resources

#### Issue: Large waveform transfers are slow

**Solutions:**
1. Use binary format instead of ASCII: `scope.write("WAV:FORM BYTE")`
2. Reduce number of data points if full resolution not needed
3. Use USB connection for faster data transfer

## Installation and Dependencies

### Python Environment Issues

#### Issue: Module import errors

**Solutions:**
1. Verify Python version (3.7+): `python --version`
2. Install all requirements: `pip install -r requirements.txt`
3. Check virtual environment activation
4. Verify package installation: `pip list`

#### Issue: Matplotlib not available for plots

**Solution:**
```bash
pip install matplotlib
```

#### Issue: NumPy compatibility problems

**Solutions:**
1. Update NumPy: `pip install --upgrade numpy`
2. Check NumPy version compatibility
3. Use virtual environment to avoid conflicts

## Hardware-Specific Issues

### DHO914S Specific Problems

#### Issue: Some features not working

**Solutions:**
1. Check firmware version compatibility
2. Update oscilloscope firmware if available
3. Some features may require specific firmware versions

#### Issue: Measurement results seem incorrect

**Solutions:**
1. Check probe compensation and calibration
2. Verify input signal is within measurement range
3. Allow settling time after changing settings
4. Check for ground loops or noise issues

## Debugging Tips

### Enable Verbose Output

```python
# For detailed error information
import logging
logging.basicConfig(level=logging.DEBUG)
```

### Manual VISA Resource Testing

```python
import pyvisa

# List all available resources
rm = pyvisa.ResourceManager()
resources = rm.list_resources()
print("Available resources:", resources)

# Test direct connection
try:
    inst = rm.open_resource('USB::0x1AB1::0x04CE::DHO9XXXX::INSTR')
    print("Identity:", inst.query('*IDN?'))
    inst.close()
except Exception as e:
    print("Error:", e)
```

### Check Oscilloscope Settings

```python
with RigolDHO914S() as scope:
    # Get comprehensive status
    status = scope.get_system_status()
    for key, value in status.items():
        print(f"{key}: {value}")
```

## Common Error Messages

### "VISA: (VI_ERROR_TMO): Timeout expired before operation completed"
- Increase timeout value
- Check if oscilloscope is responding
- Verify command syntax

### "VISA: (VI_ERROR_RSRC_NFOUND): Insufficient location information"
- Check device connection
- Verify resource string format
- Install proper drivers

### "VISA: (VI_ERROR_INV_OBJECT): The given session or object reference is invalid"
- Resource may have been closed unexpectedly
- Restart application and reconnect

## Getting Help

If problems persist:

1. Check the oscilloscope manual for SCPI command reference
2. Verify firmware version and compatibility
3. Test with official Rigol software first
4. Contact technical support with specific error messages
5. Check GitHub issues for similar problems

## Useful VISA Commands for Debugging

```python
# Check VISA installation
python -c "import pyvisa; print(pyvisa.__version__)"

# List all VISA resources
python -c "import pyvisa; rm = pyvisa.ResourceManager(); print(rm.list_resources())"

# Test basic communication
python -c "
import pyvisa
rm = pyvisa.ResourceManager()
inst = rm.open_resource('YOUR_RESOURCE_STRING')
print(inst.query('*IDN?'))
inst.close()
"
```
