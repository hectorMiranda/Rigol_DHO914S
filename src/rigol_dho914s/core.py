"""
Core Rigol DHO914S oscilloscope control class.
"""

import time
import io
from typing import Optional, Union, Tuple, List, Dict, Any
import numpy as np

try:
    import pyvisa
    PYVISA_AVAILABLE = True
except ImportError:
    PYVISA_AVAILABLE = False

from .commands import SCPICommands, MeasurementTypes
from .exceptions import RigolError, ConnectionError, CommandError, TimeoutError, DataError
from .utils import (
    parse_waveform_preamble, convert_raw_data_to_voltage, create_time_array,
    validate_channel, validate_coupling, validate_trigger_slope,
    retry_on_failure, find_usb_devices, save_waveform_csv
)


class RigolDHO914S:
    """
    Main class for controlling the Rigol DHO914S oscilloscope.
    
    Supports both USB and Ethernet connections using PyVISA.
    """
    
    def __init__(self, connection_type: str = 'usb', resource_string: Optional[str] = None,
                 ip_address: Optional[str] = None, timeout: float = 10000):
        """
        Initialize connection to the oscilloscope.
        
        Args:
            connection_type: 'usb' or 'ethernet'
            resource_string: Specific VISA resource string (optional for USB auto-detect)
            ip_address: IP address for Ethernet connection
            timeout: Command timeout in milliseconds (default: 10000 = 10 seconds)
        """
        if not PYVISA_AVAILABLE:
            raise ImportError("PyVISA is required. Install with: pip install pyvisa pyvisa-py")
        
        self.connection_type = connection_type.lower()
        self.timeout = timeout
        self.instrument = None
        self.resource_manager = None
        
        # Initialize connection
        self._connect(resource_string, ip_address)
        
        # Verify connection and get device info
        self._verify_connection()
    
    def _connect(self, resource_string: Optional[str], ip_address: Optional[str]) -> None:
        """Establish connection to the oscilloscope."""
        try:
            self.resource_manager = pyvisa.ResourceManager()
            
            if self.connection_type == 'usb':
                if resource_string:
                    # Use provided resource string
                    self.instrument = self.resource_manager.open_resource(resource_string)
                else:
                    # Auto-detect USB device
                    devices = find_usb_devices()
                    if not devices:
                        raise ConnectionError("No Rigol USB devices found")
                    
                    # Try connecting to the first available device
                    self.instrument = self.resource_manager.open_resource(devices[0])
            
            elif self.connection_type == 'ethernet':
                if not ip_address:
                    raise ValueError("IP address is required for Ethernet connection")
                
                resource_string = f"TCPIP::{ip_address}::INSTR"
                self.instrument = self.resource_manager.open_resource(resource_string)
            
            else:
                raise ValueError("Connection type must be 'usb' or 'ethernet'")
            
            # Configure instrument settings
            self.instrument.timeout = self.timeout
            self.instrument.write_termination = '\n'
            self.instrument.read_termination = '\n'
            
        except Exception as e:
            raise ConnectionError(f"Failed to connect to oscilloscope: {str(e)}")
    
    def _verify_connection(self) -> None:
        """Verify connection and check device identity."""
        try:
            identity = self.query(SCPICommands.IDENTITY)
            if 'DHO914S' not in identity:
                raise ConnectionError(f"Connected device is not a DHO914S: {identity}")
            
            print(f"Connected to: {identity}")
            
        except Exception as e:
            raise ConnectionError(f"Failed to verify connection: {str(e)}")
    
    def close(self) -> None:
        """Close the connection to the oscilloscope."""
        if self.instrument:
            self.instrument.close()
        if self.resource_manager:
            self.resource_manager.close()
    
    def __enter__(self):
        """Context manager entry."""
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit."""
        self.close()
    
    @retry_on_failure(max_retries=3)
    def write(self, command: str) -> None:
        """
        Send a command to the oscilloscope.
        
        Args:
            command: SCPI command string
        """
        try:
            self.instrument.write(command)
        except Exception as e:
            raise CommandError(f"Failed to send command '{command}': {str(e)}")
    
    @retry_on_failure(max_retries=3)
    def query(self, command: str) -> str:
        """
        Send a query command and return the response.
        
        Args:
            command: SCPI query command
            
        Returns:
            Response string from oscilloscope
        """
        try:
            response = self.instrument.query(command)
            return response.strip()
        except Exception as e:
            raise CommandError(f"Failed to query '{command}': {str(e)}")
    
    @retry_on_failure(max_retries=3)
    def query_binary(self, command: str) -> bytes:
        """
        Send a query command and return binary data.
        
        Args:
            command: SCPI query command
            
        Returns:
            Binary response from oscilloscope
        """
        try:
            response = self.instrument.query_binary_values(command, datatype='s')
            return b''.join(response)
        except Exception as e:
            raise CommandError(f"Failed to query binary '{command}': {str(e)}")
    
    def get_identity(self) -> str:
        """Get oscilloscope identity string."""
        return self.query(SCPICommands.IDENTITY)
    
    def reset(self) -> None:
        """Reset oscilloscope to default settings."""
        self.write(SCPICommands.RESET)
        time.sleep(3)  # Wait for reset to complete
    
    def clear_status(self) -> None:
        """Clear the oscilloscope status."""
        self.write(SCPICommands.CLEAR_STATUS)
    
    def get_error(self) -> str:
        """Get the last error from the oscilloscope."""
        return self.query(SCPICommands.ERROR_QUERY)
    
    def run(self) -> None:
        """Start oscilloscope acquisition."""
        self.write(SCPICommands.RUN)
    
    def stop(self) -> None:
        """Stop oscilloscope acquisition."""
        self.write(SCPICommands.STOP)
    
    def single(self) -> None:
        """Set oscilloscope to single trigger mode."""
        self.write(SCPICommands.SINGLE)
    
    def force_trigger(self) -> None:
        """Force a trigger event."""
        self.write(SCPICommands.FORCE_TRIGGER)
    
    # Channel control methods
    def set_channel_enable(self, channel: int, enable: bool) -> None:
        """Enable or disable a channel."""
        validate_channel(channel)
        state = "ON" if enable else "OFF"
        self.write(SCPICommands.CHANNEL_ENABLE.format(channel, state))
    
    def set_channel_coupling(self, channel: int, coupling: str) -> None:
        """Set channel coupling (AC, DC, GND)."""
        validate_channel(channel)
        coupling = validate_coupling(coupling)
        self.write(SCPICommands.CHANNEL_COUPLING.format(channel, coupling))
    
    def set_channel_scale(self, channel: int, scale: float) -> None:
        """Set channel vertical scale (volts/division)."""
        validate_channel(channel)
        self.write(SCPICommands.CHANNEL_SCALE.format(channel, scale))
    
    def set_channel_offset(self, channel: int, offset: float) -> None:
        """Set channel vertical offset."""
        validate_channel(channel)
        self.write(SCPICommands.CHANNEL_OFFSET.format(channel, offset))
    
    def set_channel_probe(self, channel: int, ratio: float) -> None:
        """Set channel probe ratio."""
        validate_channel(channel)
        self.write(SCPICommands.CHANNEL_PROBE.format(channel, ratio))
    
    def get_channel_scale(self, channel: int) -> float:
        """Get channel vertical scale."""
        validate_channel(channel)
        return float(self.query(SCPICommands.CHANNEL_SCALE_QUERY.format(channel)))
    
    def get_channel_offset(self, channel: int) -> float:
        """Get channel vertical offset."""
        validate_channel(channel)
        return float(self.query(SCPICommands.CHANNEL_OFFSET_QUERY.format(channel)))
    
    def get_channel_coupling(self, channel: int) -> str:
        """Get channel coupling setting."""
        validate_channel(channel)
        return self.query(SCPICommands.CHANNEL_COUPLING_QUERY.format(channel))
    
    # Timebase control methods
    def set_timebase_scale(self, scale: float) -> None:
        """Set horizontal timebase scale (seconds/division)."""
        self.write(SCPICommands.TIMEBASE_SCALE.format(scale))
    
    def set_timebase_offset(self, offset: float) -> None:
        """Set horizontal timebase offset."""
        self.write(SCPICommands.TIMEBASE_OFFSET.format(offset))
    
    def get_timebase_scale(self) -> float:
        """Get horizontal timebase scale."""
        return float(self.query(SCPICommands.TIMEBASE_SCALE_QUERY))
    
    def get_timebase_offset(self) -> float:
        """Get horizontal timebase offset."""
        return float(self.query(SCPICommands.TIMEBASE_OFFSET_QUERY))
    
    # Trigger control methods
    def set_trigger_source(self, source: str) -> None:
        """Set trigger source (CHAN1, CHAN2, CHAN3, CHAN4, EXT, AC, etc.)."""
        self.write(SCPICommands.TRIGGER_SOURCE.format(source))
    
    def set_trigger_level(self, level: float) -> None:
        """Set trigger level in volts."""
        self.write(SCPICommands.TRIGGER_LEVEL.format(level))
    
    def set_trigger_slope(self, slope: str) -> None:
        """Set trigger slope (POS, NEG, RFAL)."""
        slope = validate_trigger_slope(slope)
        self.write(SCPICommands.TRIGGER_SLOPE.format(slope))
    
    def get_trigger_status(self) -> str:
        """Get current trigger status."""
        return self.query(SCPICommands.TRIGGER_STATUS)
    
    # Screenshot methods
    def take_screenshot(self, filename: str, format: str = 'PNG') -> None:
        """
        Take a screenshot of the oscilloscope display.
        
        Args:
            filename: Output filename
            format: Image format (PNG, BMP, JPEG) - Note: DHO914S primarily supports PNG
        """
        if not self.instrument:
            raise Exception("Not connected to oscilloscope")
        
        try:
            # Store original settings
            original_timeout = self.instrument.timeout
            original_read_term = self.instrument.read_termination
            original_write_term = self.instrument.write_termination
            
            # Configure for binary mode with appropriate timeout
            self.instrument.timeout = 17000  # 15 second timeout
            self.instrument.read_termination = None  # Binary mode
            self.instrument.write_termination = '\n'
            
            # Clear any errors first
            self.instrument.write('*CLS')
            import time
            time.sleep(0.5)
            
            # Check if scope is ready
            opc = self.instrument.query('*OPC?')
            print(f"Scope ready: {opc.strip()}")
            
            # Restore binary mode after text query
            self.instrument.read_termination = None
            
            # Send the screenshot command - DHO914S works best with simple DISP:DATA?
            print("Taking screenshot...")
            self.instrument.write('DISP:DATA?')
            
            # Wait a moment for the scope to prepare the data
            time.sleep(1)
            
            # Read data in chunks to avoid timeout
            image_data = b''
            chunk_size = 1024  # 1KB chunks work well
            max_attempts = 300  # Allow for larger images (300KB max)
            
            print("Reading screenshot data...")
            
            for attempt in range(max_attempts):
                try:
                    chunk = self.instrument.read_bytes(chunk_size)
                    if not chunk:
                        break
                    
                    image_data += chunk
                    
                    # Check if we found PNG signature and have enough data
                    if b'\x89PNG' in image_data:
                        # Look for PNG end marker (IEND) to know when we have complete image
                        if b'IEND\xae\x42\x60\x82' in image_data:
                            print(f"Complete PNG detected: {len(image_data)} bytes")
                            break
                    
                    # Progress indicator for large images
                    if attempt % 20 == 0 and attempt > 0:
                        print(f"Read {len(image_data)} bytes...")
                
                except pyvisa.VisaIOError as e:
                    if 'timeout' in str(e).lower():
                        # Timeout is normal when no more data is available
                        print(f"Read complete (timeout after {len(image_data)} bytes)")
                        break
                    else:
                        raise e
            
            # Restore original settings
            self.instrument.timeout = original_timeout
            self.instrument.read_termination = original_read_term
            self.instrument.write_termination = original_write_term
            
            # Validate we got image data
            if len(image_data) < 1000:
                raise Exception(f"Screenshot data too small: {len(image_data)} bytes")
            
            # Look for PNG signature and extract image
            if b'\x89PNG' in image_data:
                png_start = image_data.find(b'\x89PNG')
                final_image = image_data[png_start:]
                print(f"Extracted PNG image: {len(final_image)} bytes")
            else:
                # If no PNG signature, save raw data (might be different format)
                final_image = image_data
                print(f"No PNG signature found, saving raw data: {len(final_image)} bytes")
            
            # Save to file
            with open(filename, 'wb') as f:
                f.write(final_image)
            
            print(f"Screenshot saved as {filename} ({len(final_image)} bytes)")
            
        except Exception as e:
            # Ensure settings are restored even on error
            try:
                self.instrument.timeout = original_timeout
                self.instrument.read_termination = original_read_term
                self.instrument.write_termination = original_write_term
            except:
                pass
            raise CommandError(f"Failed to take screenshot: {str(e)}")
    
    # Waveform acquisition methods
    def get_waveform_data(self, channel: int, points: Optional[int] = None) -> Dict[str, np.ndarray]:
        """
        Get waveform data from specified channel.
        
        Args:
            channel: Channel number (1-4)
            points: Number of points to acquire (None for all available)
            
        Returns:
            Dictionary with 'time' and 'voltage' arrays
        """
        validate_channel(channel)
        
        try:
            # Set waveform source
            self.write(SCPICommands.WAVEFORM_SOURCE.format(f"CHAN{channel}"))
            
            # Set waveform format to BYTE for efficiency
            self.write(SCPICommands.WAVEFORM_FORMAT.format("BYTE"))
            
            # Set waveform mode to NORMAL
            self.write(SCPICommands.WAVEFORM_MODE.format("NORM"))
            
            # Set data range if specified
            if points:
                self.write(SCPICommands.WAVEFORM_START.format(1))
                self.write(SCPICommands.WAVEFORM_STOP.format(points))
            
            # Get waveform preamble
            preamble_str = self.query(SCPICommands.WAVEFORM_PREAMBLE)
            preamble = parse_waveform_preamble(preamble_str)
            
            # Get waveform data
            raw_data = self.query_binary(SCPICommands.WAVEFORM_DATA)
            
            # Convert to voltage
            voltage_data = convert_raw_data_to_voltage(raw_data, preamble)
            
            # Create time array
            time_data = create_time_array(preamble)
            
            return {
                'time': time_data,
                'voltage': voltage_data,
                'preamble': preamble
            }
            
        except Exception as e:
            raise DataError(f"Failed to get waveform data from channel {channel}: {str(e)}")
    
    def save_waveform_csv(self, waveform_data: Dict[str, np.ndarray], filename: str, channel: int = 1) -> None:
        """Save waveform data to CSV file."""
        save_waveform_csv(waveform_data['time'], waveform_data['voltage'], filename, channel)
    
    # Measurement methods
    def measure(self, measurement_type: str, channel: int) -> float:
        """
        Perform automatic measurement.
        
        Args:
            measurement_type: Type of measurement (see MeasurementTypes class)
            channel: Channel number
            
        Returns:
            Measurement value
        """
        validate_channel(channel)
        
        try:
            result = self.query(SCPICommands.MEASURE_ITEM.format(measurement_type, channel))
            return float(result)
        except ValueError:
            raise DataError(f"Invalid measurement result: {result}")
    
    def get_voltage_measurements(self, channel: int) -> Dict[str, float]:
        """Get common voltage measurements for a channel."""
        measurements = {}
        
        voltage_types = [
            ('vpp', MeasurementTypes.VOLTAGE_PP),
            ('vmax', MeasurementTypes.VOLTAGE_MAX),
            ('vmin', MeasurementTypes.VOLTAGE_MIN),
            ('vrms', MeasurementTypes.VOLTAGE_RMS),
            ('vavg', MeasurementTypes.VOLTAGE_AVERAGE)
        ]
        
        for name, meas_type in voltage_types:
            try:
                measurements[name] = self.measure(meas_type, channel)
            except:
                measurements[name] = None
        
        return measurements
    
    def get_time_measurements(self, channel: int) -> Dict[str, float]:
        """Get common time measurements for a channel."""
        measurements = {}
        
        time_types = [
            ('frequency', MeasurementTypes.FREQUENCY),
            ('period', MeasurementTypes.PERIOD),
            ('rise_time', MeasurementTypes.RISE_TIME),
            ('fall_time', MeasurementTypes.FALL_TIME),
            ('pulse_width', MeasurementTypes.PULSE_WIDTH_POSITIVE)
        ]
        
        for name, meas_type in time_types:
            try:
                measurements[name] = self.measure(meas_type, channel)
            except:
                measurements[name] = None
        
        return measurements
    
    # Utility methods
    def wait_for_operation_complete(self, timeout: float = 10.0) -> None:
        """Wait for the oscilloscope to complete current operation."""
        start_time = time.time()
        while time.time() - start_time < timeout:
            if self.query("*OPC?") == "1":
                return
            time.sleep(0.1)
        raise TimeoutError("Operation did not complete within timeout")
    
    def get_system_status(self) -> Dict[str, Any]:
        """Get comprehensive system status information."""
        status = {
            'identity': self.get_identity(),
            'error': self.get_error(),
            'trigger_status': self.get_trigger_status(),
            'timebase_scale': self.get_timebase_scale(),
            'timebase_offset': self.get_timebase_offset(),
        }
        
        # Get channel information
        for channel in range(1, 5):
            try:
                status[f'ch{channel}_scale'] = self.get_channel_scale(channel)
                status[f'ch{channel}_offset'] = self.get_channel_offset(channel)
                status[f'ch{channel}_coupling'] = self.get_channel_coupling(channel)
            except:
                pass  # Channel may not be available
        
        return status
