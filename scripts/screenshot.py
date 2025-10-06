#!/usr/bin/env python3
"""
Command-line screenshot tool for Rigol DHO914S oscilloscope.

Usage:
    python screenshot.py --output screenshot.png --format PNG
    python screenshot.py --multiple 5 --interval 2.0
    python screenshot.py --timestamp --format JPEG
"""

import argparse
import sys
import os

# Add the src directory to the path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from rigol_dho914s import RigolDHO914S, ConnectionError
from examples.screenshot_tool import ScreenshotTool


def main():
    """Main command-line interface for screenshot tool."""
    parser = argparse.ArgumentParser(
        description="Take screenshots from Rigol DHO914S oscilloscope",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s --output my_screenshot.png
  %(prog)s --format JPEG --timestamp
  %(prog)s --multiple 5 --interval 2.0
  %(prog)s --settings --output measurement_capture
  %(prog)s --ethernet 192.168.1.100 --output network_screenshot.png
        """
    )
    
    # Output options
    parser.add_argument(
        "-o", "--output",
        help="Output filename (auto-generated if not specified)"
    )
    
    parser.add_argument(
        "-f", "--format",
        choices=['PNG', 'BMP', 'JPEG'],
        default='PNG',
        help="Image format (default: PNG)"
    )
    
    parser.add_argument(
        "-t", "--timestamp",
        action='store_true',
        help="Add timestamp to filename"
    )
    
    # Multiple screenshot options
    parser.add_argument(
        "-m", "--multiple",
        type=int,
        metavar='COUNT',
        help="Take multiple screenshots"
    )
    
    parser.add_argument(
        "-i", "--interval",
        type=float,
        default=1.0,
        help="Interval between multiple screenshots in seconds (default: 1.0)"
    )
    
    # Advanced options
    parser.add_argument(
        "-s", "--settings",
        action='store_true',
        help="Save oscilloscope settings along with screenshot"
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
        default=15.0,
        help="Connection timeout in seconds (default: 15.0)"
    )
    
    # Verbose output
    parser.add_argument(
        "-v", "--verbose",
        action='store_true',
        help="Enable verbose output"
    )
    
    args = parser.parse_args()
    
    # Validate arguments
    if args.multiple and args.multiple < 1:
        parser.error("Multiple count must be positive")
    
    if args.interval <= 0:
        parser.error("Interval must be positive")
    
    try:
        # Create screenshot tool
        if args.ethernet:
            if args.verbose:
                print(f"Connecting via Ethernet to {args.ethernet}...")
            tool = ScreenshotTool(connection_type='ethernet', 
                                ip_address=args.ethernet, 
                                timeout=args.timeout*1000)
        else:
            if args.verbose:
                print("Connecting via USB...")
            tool = ScreenshotTool(connection_type='usb', timeout=args.timeout*1000)
        
        # Execute based on command-line arguments
        if args.multiple:
            print(f"Taking {args.multiple} screenshots at {args.interval}s intervals...")
            tool.take_multiple_screenshots(
                count=args.multiple,
                interval=args.interval,
                format=args.format
            )
        
        elif args.settings:
            if args.verbose:
                print("Taking screenshot with settings information...")
            screenshot_file, settings_file = tool.capture_with_settings_info(
                filename=args.output,
                format=args.format
            )
            if screenshot_file:
                print(f"Screenshot: {screenshot_file}")
                print(f"Settings: {settings_file}")
        
        else:
            if args.verbose:
                print("Taking single screenshot...")
            result = tool.take_screenshot(
                filename=args.output,
                format=args.format,
                timestamp=args.timestamp
            )
            if result:
                print(f"Screenshot saved: {result}")
    
    except ConnectionError as e:
        print(f"Connection error: {e}", file=sys.stderr)
        print("Make sure the oscilloscope is connected and turned on.", file=sys.stderr)
        sys.exit(1)
    
    except KeyboardInterrupt:
        print("\nOperation cancelled by user.", file=sys.stderr)
        sys.exit(1)
    
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        if args.verbose:
            import traceback
            traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
