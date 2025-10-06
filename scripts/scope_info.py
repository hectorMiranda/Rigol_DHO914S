#!/usr/bin/env python3
"""
Device information tool for Rigol DHO914S oscilloscope.

Displays comprehensive information about the connected oscilloscope
including identity, settings, and current status.
"""

import argparse
import sys
import os
import json

# Add the src directory to the path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from rigol_dho914s import RigolDHO914S, ConnectionError


def format_value(value, unit=""):
    """Format a value with appropriate units."""
    if isinstance(value, float):
        if abs(value) >= 1e9:
            return f"{value/1e9:.3f} G{unit}"
        elif abs(value) >= 1e6:
            return f"{value/1e6:.3f} M{unit}"
        elif abs(value) >= 1e3:
            return f"{value/1e3:.3f} k{unit}"
        elif abs(value) >= 1:
            return f"{value:.3f} {unit}"
        elif abs(value) >= 1e-3:
            return f"{value*1e3:.3f} m{unit}"
        elif abs(value) >= 1e-6:
            return f"{value*1e6:.3f} µ{unit}"
        elif abs(value) >= 1e-9:
            return f"{value*1e9:.3f} n{unit}"
        else:
            return f"{value:.6f} {unit}"
    return str(value)


def display_device_info(scope, verbose=False):
    """Display basic device information."""
    print("DEVICE INFORMATION")
    print("=" * 50)
    
    try:
        identity = scope.get_identity()
        print(f"Device: {identity}")
        
        # Parse identity string
        if ',' in identity:
            parts = identity.split(',')
            if len(parts) >= 4:
                manufacturer = parts[0].strip()
                model = parts[1].strip()
                serial = parts[2].strip()
                firmware = parts[3].strip()
                
                print(f"Manufacturer: {manufacturer}")
                print(f"Model: {model}")
                print(f"Serial Number: {serial}")
                print(f"Firmware: {firmware}")
        
        # Check for errors
        error = scope.get_error()
        if error and "No error" not in error:
            print(f"⚠️  Current Error: {error}")
        else:
            print("✅ No errors reported")
            
    except Exception as e:
        print(f"❌ Error getting device info: {e}")


def display_channel_info(scope, verbose=False):
    """Display channel configuration information."""
    print("\nCHANNEL CONFIGURATION")
    print("=" * 50)
    
    for channel in range(1, 5):
        try:
            print(f"\nChannel {channel}:")
            
            scale = scope.get_channel_scale(channel)
            offset = scope.get_channel_offset(channel)
            coupling = scope.get_channel_coupling(channel)
            probe = scope.get_channel_probe(channel)
            
            print(f"  Scale: {format_value(scale, 'V/div')}")
            print(f"  Offset: {format_value(offset, 'V')}")
            print(f"  Coupling: {coupling}")
            print(f"  Probe Ratio: {probe}:1")
            
        except Exception as e:
            if verbose:
                print(f"  ❌ Error reading channel {channel}: {e}")
            else:
                print(f"  Channel {channel}: Not available")


def display_timebase_info(scope, verbose=False):
    """Display timebase information."""
    print("\nTIMEBASE CONFIGURATION")
    print("=" * 50)
    
    try:
        scale = scope.get_timebase_scale()
        offset = scope.get_timebase_offset()
        
        print(f"Scale: {format_value(scale, 's/div')}")
        print(f"Offset: {format_value(offset, 's')}")
        
        # Calculate total time span (typically 12 divisions)
        total_time = scale * 12
        print(f"Total Time Span: {format_value(total_time, 's')}")
        
    except Exception as e:
        print(f"❌ Error reading timebase: {e}")


def display_trigger_info(scope, verbose=False):
    """Display trigger information."""
    print("\nTRIGGER STATUS")
    print("=" * 50)
    
    try:
        status = scope.get_trigger_status()
        print(f"Status: {status}")
        
        # Try to get additional trigger information
        if verbose:
            try:
                # These commands might not be available on all firmware versions
                source = scope.query("TRIG:EDGE:SOUR?")
                level = scope.query("TRIG:EDGE:LEV?")
                slope = scope.query("TRIG:EDGE:SLOP?")
                
                print(f"Source: {source}")
                print(f"Level: {format_value(float(level), 'V')}")
                print(f"Slope: {slope}")
                
            except:
                pass  # Additional info not available
                
    except Exception as e:
        print(f"❌ Error reading trigger status: {e}")


def display_measurements(scope, channels=[1], verbose=False):
    """Display automatic measurements for specified channels."""
    print("\nAUTOMATIC MEASUREMENTS")
    print("=" * 50)
    
    for channel in channels:
        try:
            print(f"\nChannel {channel} Measurements:")
            
            # Get voltage measurements
            voltage_measurements = scope.get_voltage_measurements(channel)
            
            if any(v is not None for v in voltage_measurements.values()):
                print("  Voltage:")
                for name, value in voltage_measurements.items():
                    if value is not None:
                        formatted_name = name.replace('_', ' ').title()
                        print(f"    {formatted_name}: {format_value(value, 'V')}")
            
            # Get time measurements
            time_measurements = scope.get_time_measurements(channel)
            
            if any(v is not None for v in time_measurements.values()):
                print("  Timing:")
                for name, value in time_measurements.items():
                    if value is not None:
                        formatted_name = name.replace('_', ' ').title()
                        if name == 'frequency':
                            print(f"    {formatted_name}: {format_value(value, 'Hz')}")
                        else:
                            print(f"    {formatted_name}: {format_value(value, 's')}")
            
        except Exception as e:
            if verbose:
                print(f"  ❌ Error getting measurements for channel {channel}: {e}")
            else:
                print(f"  Channel {channel}: Measurements not available")


def save_info_to_file(scope, filename, format_type='txt'):
    """Save device information to file."""
    try:
        status = scope.get_system_status()
        
        if format_type.lower() == 'json':
            # Save as JSON
            with open(filename, 'w') as f:
                json.dump(status, f, indent=2, default=str)
        else:
            # Save as text
            with open(filename, 'w') as f:
                f.write("Rigol DHO914S Device Information\n")
                f.write(f"Timestamp: {__import__('datetime').datetime.now()}\n")
                f.write("=" * 60 + "\n\n")
                
                for key, value in status.items():
                    f.write(f"{key}: {value}\n")
        
        print(f"Device information saved to {filename}")
        
    except Exception as e:
        print(f"❌ Error saving to file: {e}")


def main():
    """Main command-line interface."""
    parser = argparse.ArgumentParser(
        description="Display Rigol DHO914S oscilloscope information",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s                          # Basic device info
  %(prog)s --verbose                # Detailed information
  %(prog)s --measurements 1,2       # Include measurements for channels 1 and 2
  %(prog)s --save info.txt          # Save information to file
  %(prog)s --ethernet 192.168.1.100 # Connect via Ethernet
        """
    )
    
    # Connection options
    parser.add_argument(
        "--ethernet",
        metavar='IP_ADDRESS',
        help="Use Ethernet connection with specified IP address"
    )
    
    parser.add_argument(
        "--timeout",
        type=float,
        default=5.0,
        help="Connection timeout in seconds (default: 5.0)"
    )
    
    # Display options
    parser.add_argument(
        "-v", "--verbose",
        action='store_true',
        help="Show detailed information and error messages"
    )
    
    parser.add_argument(
        "-m", "--measurements",
        metavar='CHANNELS',
        help="Show measurements for specified channels (e.g., '1,2,3')"
    )
    
    parser.add_argument(
        "--no-channels",
        action='store_true',
        help="Skip channel information display"
    )
    
    parser.add_argument(
        "--no-timebase",
        action='store_true',
        help="Skip timebase information display"
    )
    
    parser.add_argument(
        "--no-trigger",
        action='store_true',
        help="Skip trigger information display"
    )
    
    # Output options
    parser.add_argument(
        "-s", "--save",
        metavar='FILENAME',
        help="Save device information to file"
    )
    
    parser.add_argument(
        "--format",
        choices=['txt', 'json'],
        default='txt',
        help="Output format for saved file (default: txt)"
    )
    
    args = parser.parse_args()
    
    # Parse measurement channels
    measurement_channels = []
    if args.measurements:
        try:
            measurement_channels = [int(ch.strip()) for ch in args.measurements.split(',')]
            # Validate channel numbers
            for ch in measurement_channels:
                if ch < 1 or ch > 4:
                    parser.error(f"Invalid channel number: {ch} (must be 1-4)")
        except ValueError:
            parser.error("Invalid channel format. Use comma-separated numbers (e.g., '1,2,3')")
    
    try:
        # Connect to oscilloscope
        if args.ethernet:
            if args.verbose:
                print(f"Connecting via Ethernet to {args.ethernet}...")
            scope = RigolDHO914S(connection_type='ethernet', ip_address=args.ethernet, timeout=args.timeout*1000)
        else:
            if args.verbose:
                print("Connecting via USB...")
            scope = RigolDHO914S(connection_type='usb', timeout=args.timeout*1000)
        
        with scope:
            # Display information sections
            display_device_info(scope, args.verbose)
            
            if not args.no_channels:
                display_channel_info(scope, args.verbose)
            
            if not args.no_timebase:
                display_timebase_info(scope, args.verbose)
            
            if not args.no_trigger:
                display_trigger_info(scope, args.verbose)
            
            if measurement_channels:
                display_measurements(scope, measurement_channels, args.verbose)
            
            # Save to file if requested
            if args.save:
                save_info_to_file(scope, args.save, args.format)
    
    except ConnectionError as e:
        print(f"❌ Connection error: {e}", file=sys.stderr)
        print("Make sure the oscilloscope is connected and turned on.", file=sys.stderr)
        sys.exit(1)
    
    except KeyboardInterrupt:
        print("\n❌ Operation cancelled by user.", file=sys.stderr)
        sys.exit(1)
    
    except Exception as e:
        print(f"❌ Error: {e}", file=sys.stderr)
        if args.verbose:
            import traceback
            traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
