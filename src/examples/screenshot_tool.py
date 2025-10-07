"""
Screenshot utility for Rigol DHO914S oscilloscope.

This script provides a simple interface for taking screenshots
with various options and formats.
"""

import sys
import os
import datetime
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from rigol_dho914s import RigolDHO914S


class ScreenshotTool:
    """Tool for taking oscilloscope screenshots."""
    
    def __init__(self, connection_type='usb', ip_address=None, timeout=15000, output_dir='outputs'):
        """Initialize the screenshot tool."""
        self.connection_type = connection_type
        self.ip_address = ip_address
        self.timeout = timeout
        self.output_dir = output_dir
        
        # Create output directory if it doesn't exist
        if not os.path.exists(self.output_dir):
            os.makedirs(self.output_dir)
    
    def take_screenshot(self, filename=None, format='PNG', timestamp=False):
        """
        Take a screenshot with specified options.
        
        Args:
            filename: Output filename (auto-generated if None)
            format: Image format (PNG, BMP, JPEG)
            timestamp: Add timestamp to filename
        """
        try:
            with RigolDHO914S(connection_type=self.connection_type, 
                            ip_address=self.ip_address,
                            timeout=self.timeout) as scope:
                
                # Generate filename if not provided
                if filename is None:
                    timestamp_str = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
                    # Always use timestamp for auto-generated filenames to avoid overwrites
                    filename = f"screenshot_{timestamp_str}.{format.lower()}"
                
                # Add timestamp to existing filename if requested
                elif timestamp:
                    name, ext = os.path.splitext(filename)
                    timestamp_str = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
                    filename = f"{name}_{timestamp_str}{ext}"
                
                # Ensure filename is in the output directory
                if not os.path.isabs(filename):
                    filename = os.path.join(self.output_dir, filename)
                
                print(f"Taking screenshot...")
                print(f"Format: {format}")
                print(f"Output: {filename}")
                
                # Get device info
                identity = scope.get_identity()
                print(f"Device: {identity}")
                
                # Take screenshot
                scope.take_screenshot(filename, format=format)
                
                print(f"Screenshot saved successfully!")
                return filename
                
        except Exception as e:
            print(f"Error taking screenshot: {e}")
            return None
    
    def take_multiple_screenshots(self, count=5, interval=1.0, format='PNG'):
        """
        Take multiple screenshots at regular intervals.
        
        Args:
            count: Number of screenshots to take
            interval: Time between screenshots in seconds
            format: Image format
        """
        import time
        
        print(f"Taking {count} screenshots at {interval}s intervals...")
        
        for i in range(count):
            timestamp_str = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"screenshot_{i+1:03d}_{timestamp_str}.{format.lower()}"
            filename = os.path.join(self.output_dir, filename)
            
            print(f"Screenshot {i+1}/{count}: {filename}")
            result = self.take_screenshot(filename, format=format, timestamp=False)
            
            if result is None:
                print(f"Failed to take screenshot {i+1}")
                break
            
            if i < count - 1:  # Don't wait after the last screenshot
                time.sleep(interval)
        
        print("Multiple screenshot capture completed.")
    
    def capture_with_settings_info(self, filename=None, format='PNG'):
        """
        Take screenshot and save current oscilloscope settings.
        
        Args:
            filename: Base filename for screenshot and settings
            format: Image format
        """
        try:
            with RigolDHO914S(connection_type=self.connection_type,
                            ip_address=self.ip_address,
                            timeout=self.timeout) as scope:
                
                # Generate base filename
                if filename is None:
                    timestamp_str = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
                    base_name = f"capture_{timestamp_str}"
                else:
                    base_name = os.path.splitext(filename)[0]
                
                screenshot_file = f"{base_name}.{format.lower()}"
                settings_file = f"{base_name}_settings.txt"
                
                print(f"Capturing screenshot with settings...")
                
                # Take screenshot
                scope.take_screenshot(screenshot_file, format=format)
                
                # Get system status
                status = scope.get_system_status()
                
                # Save settings to text file
                with open(settings_file, 'w') as f:
                    f.write(f"Rigol DHO914S Settings Capture\n")
                    f.write(f"Timestamp: {datetime.datetime.now()}\n")
                    f.write(f"Screenshot: {screenshot_file}\n")
                    f.write("=" * 50 + "\n\n")
                    
                    for key, value in status.items():
                        f.write(f"{key}: {value}\n")
                
                print(f"Screenshot saved: {screenshot_file}")
                print(f"Settings saved: {settings_file}")
                
                return screenshot_file, settings_file
                
        except Exception as e:
            print(f"Error in capture with settings: {e}")
            return None, None


def main():
    """Main function for command-line usage."""
    import argparse
    
    parser = argparse.ArgumentParser(description="Rigol DHO914S Screenshot Tool")
    parser.add_argument("-o", "--output", help="Output filename")
    parser.add_argument("-f", "--format", choices=['PNG', 'BMP', 'JPEG'], 
                       default='PNG', help="Image format")
    parser.add_argument("-t", "--timestamp", action='store_true',
                       help="Add timestamp to filename")
    parser.add_argument("-m", "--multiple", type=int, metavar='COUNT',
                       help="Take multiple screenshots")
    parser.add_argument("-i", "--interval", type=float, default=1.0,
                       help="Interval between multiple screenshots (seconds)")
    parser.add_argument("-s", "--settings", action='store_true',
                       help="Save oscilloscope settings with screenshot")
    parser.add_argument("--ethernet", metavar='IP',
                       help="Use Ethernet connection with specified IP")
    
    args = parser.parse_args()
    
    # Create screenshot tool
    if args.ethernet:
        tool = ScreenshotTool(connection_type='ethernet', ip_address=args.ethernet)
    else:
        tool = ScreenshotTool(connection_type='usb')
    
    # Execute based on arguments
    if args.multiple:
        tool.take_multiple_screenshots(
            count=args.multiple,
            interval=args.interval,
            format=args.format
        )
    elif args.settings:
        tool.capture_with_settings_info(
            filename=args.output,
            format=args.format
        )
    else:
        tool.take_screenshot(
            filename=args.output,
            format=args.format,
            timestamp=args.timestamp
        )


if __name__ == "__main__":
    main()
