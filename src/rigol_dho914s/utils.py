"""
Utility functions for Rigol DHO914S oscilloscope operations.
"""

import time
import numpy as np
from typing import List, Tuple, Optional, Union
import struct


def parse_waveform_preamble(preamble_string: str) -> dict:
    """
    Parse the waveform preamble data returned by WAV:PRE? command.
    
    Args:
        preamble_string: Comma-separated string from oscilloscope
        
    Returns:
        Dictionary with parsed preamble data
    """
    parts = preamble_string.strip().split(',')
    
    if len(parts) < 10:
        raise ValueError("Invalid preamble format")
    
    return {
        'format': int(parts[0]),        # 0=BYTE, 1=WORD, 2=ASC
        'type': int(parts[1]),          # 0=NORMal, 1=MAXimum, 2=RAW
        'points': int(parts[2]),        # Number of data points
        'count': int(parts[3]),         # Always 1
        'x_increment': float(parts[4]), # Time between data points
        'x_origin': float(parts[5]),    # Time of first data point
        'x_reference': int(parts[6]),   # Always 0
        'y_increment': float(parts[7]), # Voltage per LSB
        'y_origin': float(parts[8]),    # Voltage at center screen
        'y_reference': int(parts[9])    # Vertical position
    }


def convert_raw_data_to_voltage(raw_data: bytes, preamble: dict) -> np.ndarray:
    """
    Convert raw waveform data to voltage values.
    
    Args:
        raw_data: Raw bytes from oscilloscope
        preamble: Parsed preamble dictionary
        
    Returns:
        NumPy array of voltage values
    """
    if preamble['format'] == 0:  # BYTE format
        # Convert bytes to signed integers (-128 to 127)
        data_array = np.frombuffer(raw_data, dtype=np.int8)
    elif preamble['format'] == 1:  # WORD format
        # Convert bytes to signed 16-bit integers
        data_array = np.frombuffer(raw_data, dtype=np.int16)
    else:
        raise ValueError(f"Unsupported data format: {preamble['format']}")
    
    # Convert to voltage using preamble scaling factors
    voltage_data = (data_array - preamble['y_reference']) * preamble['y_increment'] + preamble['y_origin']
    
    return voltage_data


def create_time_array(preamble: dict) -> np.ndarray:
    """
    Create time array corresponding to waveform data points.
    
    Args:
        preamble: Parsed preamble dictionary
        
    Returns:
        NumPy array of time values
    """
    points = preamble['points']
    x_increment = preamble['x_increment']
    x_origin = preamble['x_origin']
    
    time_array = np.arange(points) * x_increment + x_origin
    return time_array


def format_scientific(value: float, precision: int = 3) -> str:
    """
    Format a number in scientific notation with appropriate units.
    
    Args:
        value: Number to format
        precision: Number of decimal places
        
    Returns:
        Formatted string with units
    """
    if abs(value) >= 1e9:
        return f"{value/1e9:.{precision}f}G"
    elif abs(value) >= 1e6:
        return f"{value/1e6:.{precision}f}M"
    elif abs(value) >= 1e3:
        return f"{value/1e3:.{precision}f}k"
    elif abs(value) >= 1:
        return f"{value:.{precision}f}"
    elif abs(value) >= 1e-3:
        return f"{value*1e3:.{precision}f}m"
    elif abs(value) >= 1e-6:
        return f"{value*1e6:.{precision}f}µ"
    elif abs(value) >= 1e-9:
        return f"{value*1e9:.{precision}f}n"
    else:
        return f"{value*1e12:.{precision}f}p"


def voltage_to_scale_string(voltage: float) -> str:
    """
    Convert voltage value to appropriate scale string for oscilloscope.
    
    Args:
        voltage: Voltage value in volts
        
    Returns:
        String representation for oscilloscope command
    """
    return format_scientific(voltage)


def time_to_scale_string(time_val: float) -> str:
    """
    Convert time value to appropriate scale string for oscilloscope.
    
    Args:
        time_val: Time value in seconds
        
    Returns:
        String representation for oscilloscope command
    """
    return format_scientific(time_val)


def validate_channel(channel: int) -> None:
    """
    Validate channel number for DHO914S (1-4).
    
    Args:
        channel: Channel number to validate
        
    Raises:
        ValueError: If channel is not valid
    """
    if not isinstance(channel, int) or channel < 1 or channel > 4:
        raise ValueError("Channel must be an integer between 1 and 4")


def validate_coupling(coupling: str) -> str:
    """
    Validate and normalize coupling setting.
    
    Args:
        coupling: Coupling setting to validate
        
    Returns:
        Normalized coupling string
        
    Raises:
        ValueError: If coupling is not valid
    """
    coupling = coupling.upper()
    valid_couplings = ['AC', 'DC', 'GND']
    
    if coupling not in valid_couplings:
        raise ValueError(f"Coupling must be one of: {valid_couplings}")
    
    return coupling


def validate_trigger_slope(slope: str) -> str:
    """
    Validate and normalize trigger slope setting.
    
    Args:
        slope: Trigger slope to validate
        
    Returns:
        Normalized slope string
        
    Raises:
        ValueError: If slope is not valid
    """
    slope = slope.upper()
    valid_slopes = ['POS', 'NEG', 'RFAL']
    
    if slope not in valid_slopes:
        raise ValueError(f"Trigger slope must be one of: {valid_slopes}")
    
    return slope


def retry_on_failure(max_retries: int = 3, delay: float = 0.1):
    """
    Decorator to retry function calls on failure.
    
    Args:
        max_retries: Maximum number of retry attempts
        delay: Delay between retries in seconds
    """
    def decorator(func):
        def wrapper(*args, **kwargs):
            last_exception = None
            
            for attempt in range(max_retries + 1):
                try:
                    return func(*args, **kwargs)
                except Exception as e:
                    last_exception = e
                    if attempt < max_retries:
                        time.sleep(delay)
                        continue
                    else:
                        break
            
            raise last_exception
        return wrapper
    return decorator


def calculate_sample_rate(timebase_scale: float, memory_depth: int) -> float:
    """
    Calculate the effective sample rate based on timebase and memory depth.
    
    Args:
        timebase_scale: Timebase scale in seconds/division
        memory_depth: Memory depth in samples
        
    Returns:
        Sample rate in samples per second
    """
    # Total time span is typically 12 divisions
    total_time = timebase_scale * 12
    sample_rate = memory_depth / total_time
    return sample_rate


def find_usb_devices() -> List[str]:
    """
    Find connected Rigol USB devices.
    
    Returns:
        List of device resource strings
    """
    try:
        import pyvisa
        rm = pyvisa.ResourceManager()
        resources = rm.list_resources()
        
        # Filter for Rigol USB devices
        rigol_devices = []
        for resource in resources:
            if 'USB' in resource and ('RIGOL' in resource.upper() or 'DHO' in resource.upper()):
                rigol_devices.append(resource)
        
        return rigol_devices
    except ImportError:
        raise ImportError("PyVISA is required for USB device detection")


def save_waveform_csv(time_data: np.ndarray, voltage_data: np.ndarray, 
                     filename: str, channel: int = 1) -> None:
    """
    Save waveform data to CSV file.
    
    Args:
        time_data: Time array
        voltage_data: Voltage array
        filename: Output filename
        channel: Channel number for header
    """
    header = f"Time (s),Channel {channel} Voltage (V)"
    data = np.column_stack((time_data, voltage_data))
    np.savetxt(filename, data, delimiter=',', header=header, comments='')


def load_waveform_csv(filename: str) -> Tuple[np.ndarray, np.ndarray]:
    """
    Load waveform data from CSV file.
    
    Args:
        filename: Input filename
        
    Returns:
        Tuple of (time_data, voltage_data)
    """
    data = np.loadtxt(filename, delimiter=',', skiprows=1)
    return data[:, 0], data[:, 1]
