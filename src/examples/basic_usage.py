"""
Basic usage examples for Rigol DHO914S oscilloscope.

This script demonstrates fundamental operations like connecting,
taking screenshots, and basic channel configuration.
"""

import sys
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from rigol_dho914s import RigolDHO914S, ConnectionError


def basic_connection_test():
    """Test basic connection to the oscilloscope."""
    print("=== Basic Connection Test ===")
    
    try:
        # Try USB connection first
        with RigolDHO914S(connection_type='usb') as scope:
            print(f"Connected successfully!")
            print(f"Device: {scope.get_identity()}")
            
            # Check for any errors
            error = scope.get_error()
            print(f"Error status: {error}")
            
            return True
            
    except ConnectionError as e:
        print(f"Connection failed: {e}")
        return False


def take_screenshot_example():
    """Take a screenshot of the oscilloscope display."""
    print("\n=== Screenshot Example ===")
    
    try:
        with RigolDHO914S() as scope:
            # Take screenshot in PNG format
            filename = "oscilloscope_screenshot.png"
            scope.take_screenshot(filename, format='PNG')
            print(f"Screenshot saved as {filename}")
            
    except Exception as e:
        print(f"Screenshot failed: {e}")


def channel_configuration_example():
    """Configure oscilloscope channels."""
    print("\n=== Channel Configuration Example ===")
    
    try:
        with RigolDHO914S() as scope:
            # Configure Channel 1
            print("Configuring Channel 1...")
            scope.set_channel_enable(1, True)
            scope.set_channel_coupling(1, 'DC')
            scope.set_channel_scale(1, 0.5)  # 500mV/div
            scope.set_channel_offset(1, 0.0)
            scope.set_channel_probe(1, 1.0)  # 1x probe
            
            # Configure Channel 2
            print("Configuring Channel 2...")
            scope.set_channel_enable(2, True)
            scope.set_channel_coupling(2, 'AC')
            scope.set_channel_scale(2, 1.0)  # 1V/div
            scope.set_channel_offset(2, 0.0)
            
            # Set timebase
            print("Setting timebase...")
            scope.set_timebase_scale(1e-3)  # 1ms/div
            scope.set_timebase_offset(0.0)
            
            # Configure trigger
            print("Configuring trigger...")
            scope.set_trigger_source('CHAN1')
            scope.set_trigger_level(0.0)
            scope.set_trigger_slope('POS')
            
            # Get current settings
            print("\nCurrent settings:")
            print(f"CH1 Scale: {scope.get_channel_scale(1)}V/div")
            print(f"CH1 Coupling: {scope.get_channel_coupling(1)}")
            print(f"Timebase: {scope.get_timebase_scale()}s/div")
            print(f"Trigger status: {scope.get_trigger_status()}")
            
    except Exception as e:
        print(f"Configuration failed: {e}")


def acquisition_control_example():
    """Demonstrate acquisition control commands."""
    print("\n=== Acquisition Control Example ===")
    
    try:
        with RigolDHO914S() as scope:
            print("Starting acquisition...")
            scope.run()
            
            # Wait a moment
            import time
            time.sleep(2)
            
            print("Stopping acquisition...")
            scope.stop()
            
            print("Setting single trigger mode...")
            scope.single()
            
            print("Forcing trigger...")
            scope.force_trigger()
            
    except Exception as e:
        print(f"Acquisition control failed: {e}")


def system_status_example():
    """Get comprehensive system status."""
    print("\n=== System Status Example ===")
    
    try:
        with RigolDHO914S() as scope:
            status = scope.get_system_status()
            
            print("System Status:")
            print("-" * 40)
            for key, value in status.items():
                print(f"{key}: {value}")
            
    except Exception as e:
        print(f"Status query failed: {e}")


def main():
    """Run all basic examples."""
    print("Rigol DHO914S Basic Usage Examples")
    print("=" * 50)
    
    # Test connection first
    if not basic_connection_test():
        print("Cannot proceed without a valid connection.")
        return
    
    # Run examples
    take_screenshot_example()
    channel_configuration_example()
    acquisition_control_example()
    system_status_example()
    
    print("\n=== All examples completed ===")


if __name__ == "__main__":
    main()
