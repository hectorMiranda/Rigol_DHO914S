#!/usr/bin/env python3
"""
Waveform export tool for Rigol DHO914S oscilloscope.

Exports waveform data from one or more channels to various formats
including CSV, NumPy arrays, and plots.
"""

import argparse
import sys
import os
import json
import numpy as np

# Add the src directory to the path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from rigol_dho914s import RigolDHO914S, ConnectionError


def export_channel_data(scope, channel, output_dir, formats, verbose=False):
    """
    Export data from a single channel.
    
    Args:
        scope: Connected oscilloscope instance
        channel: Channel number to export
        output_dir: Output directory path
        formats: List of output formats
        verbose: Enable verbose output
    
    Returns:
        Dictionary with export results
    """
    if verbose:
        print(f"Exporting Channel {channel}...")
    
    try:
        # Ensure channel is enabled
        scope.set_channel_enable(channel, True)
        
        # Capture waveform data
        waveform = scope.get_waveform_data(channel)
        
        # Get measurements
        voltage_measurements = scope.get_voltage_measurements(channel)
        time_measurements = scope.get_time_measurements(channel)
        
        results = {
            'channel': channel,
            'points': len(waveform['voltage']),
            'files': []
        }
        
        base_filename = f"ch{channel}_waveform"
        
        # Export in requested formats
        for format_type in formats:
            if format_type == 'csv':
                filename = os.path.join(output_dir, f"{base_filename}.csv")
                scope.save_waveform_csv(waveform, filename, channel)
                results['files'].append(filename)
                if verbose:
                    print(f"  Saved CSV: {filename}")
            
            elif format_type == 'npy':
                # Save as NumPy arrays
                time_file = os.path.join(output_dir, f"{base_filename}_time.npy")
                voltage_file = os.path.join(output_dir, f"{base_filename}_voltage.npy")
                
                np.save(time_file, waveform['time'])
                np.save(voltage_file, waveform['voltage'])
                
                results['files'].extend([time_file, voltage_file])
                if verbose:
                    print(f"  Saved NumPy: {time_file}, {voltage_file}")
            
            elif format_type == 'json':
                # Save metadata and measurements as JSON
                metadata = {
                    'channel': channel,
                    'preamble': waveform['preamble'],
                    'voltage_measurements': voltage_measurements,
                    'time_measurements': time_measurements,
                    'data_points': len(waveform['voltage']),
                    'timestamp': str(__import__('datetime').datetime.now())
                }
                
                filename = os.path.join(output_dir, f"{base_filename}_metadata.json")
                with open(filename, 'w') as f:
                    json.dump(metadata, f, indent=2, default=str)
                
                results['files'].append(filename)
                if verbose:
                    print(f"  Saved metadata: {filename}")
            
            elif format_type == 'plot':
                # Create and save plot
                try:
                    import matplotlib.pyplot as plt
                    
                    plt.figure(figsize=(12, 6))
                    plt.plot(waveform['time'] * 1000, waveform['voltage'], 'b-', linewidth=1)
                    plt.xlabel('Time (ms)')
                    plt.ylabel('Voltage (V)')
                    plt.title(f'Channel {channel} Waveform')
                    plt.grid(True, alpha=0.3)
                    
                    filename = os.path.join(output_dir, f"{base_filename}_plot.png")
                    plt.savefig(filename, dpi=300, bbox_inches='tight')
                    plt.close()
                    
                    results['files'].append(filename)
                    if verbose:
                        print(f"  Saved plot: {filename}")
                        
                except ImportError:
                    if verbose:
                        print("  ⚠️  Matplotlib not available, skipping plot")
        
        # Save measurements to text file
        measurements_file = os.path.join(output_dir, f"{base_filename}_measurements.txt")
        with open(measurements_file, 'w') as f:
            f.write(f"Rigol DHO914S Channel {channel} Measurements\n")
            f.write(f"Timestamp: {__import__('datetime').datetime.now()}\n")
            f.write("=" * 50 + "\n\n")
            
            f.write(f"Data Points: {len(waveform['voltage'])}\n")
            f.write(f"Time Range: {waveform['time'][0]:.6f} to {waveform['time'][-1]:.6f} seconds\n")
            f.write(f"Sample Rate: {1/(waveform['time'][1] - waveform['time'][0]):.0f} Hz\n\n")
            
            f.write("Voltage Measurements:\n")
            for name, value in voltage_measurements.items():
                if value is not None:
                    f.write(f"  {name}: {value:.6f} V\n")
            
            f.write("\nTime Measurements:\n")
            for name, value in time_measurements.items():
                if value is not None:
                    f.write(f"  {name}: {value:.9f} s\n")
        
        results['files'].append(measurements_file)
        
        return results
        
    except Exception as e:
        print(f"❌ Error exporting channel {channel}: {e}")
        return None


def export_multiple_channels(scope, channels, output_dir, formats, verbose=False):
    """Export data from multiple channels and create combined files."""
    
    all_results = []
    all_waveforms = {}
    
    # Export individual channels
    for channel in channels:
        result = export_channel_data(scope, channel, output_dir, formats, verbose)
        if result:
            all_results.append(result)
            
            # Store waveform for combined export
            waveform = scope.get_waveform_data(channel)
            all_waveforms[channel] = waveform
    
    # Create combined exports
    if len(all_waveforms) > 1:
        if verbose:
            print("Creating combined exports...")
        
        # Combined CSV
        if 'csv' in formats:
            combined_csv = os.path.join(output_dir, "combined_waveforms.csv")
            create_combined_csv(all_waveforms, combined_csv)
            if verbose:
                print(f"  Saved combined CSV: {combined_csv}")
        
        # Combined plot
        if 'plot' in formats:
            try:
                import matplotlib.pyplot as plt
                
                combined_plot = os.path.join(output_dir, "combined_waveforms_plot.png")
                create_combined_plot(all_waveforms, combined_plot)
                if verbose:
                    print(f"  Saved combined plot: {combined_plot}")
                    
            except ImportError:
                if verbose:
                    print("  ⚠️  Matplotlib not available, skipping combined plot")
    
    return all_results


def create_combined_csv(waveforms, filename):
    """Create a CSV file with data from multiple channels."""
    # Find the common time base (use the first channel's time data)
    first_channel = list(waveforms.keys())[0]
    time_data = waveforms[first_channel]['time']
    
    # Prepare header
    header = "Time (s)"
    for channel in sorted(waveforms.keys()):
        header += f",Channel {channel} (V)"
    
    # Prepare data array
    data = [time_data]
    for channel in sorted(waveforms.keys()):
        data.append(waveforms[channel]['voltage'])
    
    # Save to CSV
    combined_data = np.column_stack(data)
    np.savetxt(filename, combined_data, delimiter=',', header=header, comments='')


def create_combined_plot(waveforms, filename):
    """Create a plot with multiple channel waveforms."""
    import matplotlib.pyplot as plt
    
    plt.figure(figsize=(14, 8))
    colors = ['blue', 'red', 'green', 'orange']
    
    for i, (channel, waveform) in enumerate(sorted(waveforms.items())):
        color = colors[i % len(colors)]
        plt.plot(waveform['time'] * 1000, waveform['voltage'], 
                color=color, linewidth=1, label=f'Channel {channel}')
    
    plt.xlabel('Time (ms)')
    plt.ylabel('Voltage (V)')
    plt.title('Multi-Channel Waveforms')
    plt.legend()
    plt.grid(True, alpha=0.3)
    
    plt.savefig(filename, dpi=300, bbox_inches='tight')
    plt.close()


def main():
    """Main command-line interface."""
    parser = argparse.ArgumentParser(
        description="Export waveform data from Rigol DHO914S oscilloscope",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s --channels 1,2 --format csv,plot --output ./data/
  %(prog)s --channels 1 --format csv,npy,json --output exports/
  %(prog)s --all-channels --format csv --output waveforms/
  %(prog)s --channels 1,2,3,4 --format plot --ethernet 192.168.1.100
        """
    )
    
    # Channel selection
    parser.add_argument(
        "-c", "--channels",
        metavar='CHANNELS',
        help="Channels to export (e.g., '1,2,3')"
    )
    
    parser.add_argument(
        "--all-channels",
        action='store_true',
        help="Export all 4 channels"
    )
    
    # Output format options
    parser.add_argument(
        "-f", "--format",
        metavar='FORMATS',
        default='csv',
        help="Export formats: csv,npy,json,plot (default: csv)"
    )
    
    parser.add_argument(
        "-o", "--output",
        metavar='DIRECTORY',
        default='.',
        help="Output directory (default: current directory)"
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
        default=10.0,
        help="Connection timeout in seconds (default: 10.0)"
    )
    
    # Acquisition options
    parser.add_argument(
        "--points",
        type=int,
        help="Number of data points to capture (default: all available)"
    )
    
    parser.add_argument(
        "--run-first",
        action='store_true',
        help="Start acquisition before capturing data"
    )
    
    # Display options
    parser.add_argument(
        "-v", "--verbose",
        action='store_true',
        help="Enable verbose output"
    )
    
    parser.add_argument(
        "--summary",
        action='store_true',
        help="Display summary of exported data"
    )
    
    args = parser.parse_args()
    
    # Parse channels
    channels = []
    if args.all_channels:
        channels = [1, 2, 3, 4]
    elif args.channels:
        try:
            channels = [int(ch.strip()) for ch in args.channels.split(',')]
            # Validate channel numbers
            for ch in channels:
                if ch < 1 or ch > 4:
                    parser.error(f"Invalid channel number: {ch} (must be 1-4)")
        except ValueError:
            parser.error("Invalid channel format. Use comma-separated numbers (e.g., '1,2,3')")
    else:
        parser.error("Must specify --channels or --all-channels")
    
    # Parse formats
    formats = [fmt.strip().lower() for fmt in args.format.split(',')]
    valid_formats = ['csv', 'npy', 'json', 'plot']
    for fmt in formats:
        if fmt not in valid_formats:
            parser.error(f"Invalid format: {fmt}. Valid formats: {', '.join(valid_formats)}")
    
    # Create output directory
    output_dir = os.path.abspath(args.output)
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
        if args.verbose:
            print(f"Created output directory: {output_dir}")
    
    try:
        # Connect to oscilloscope
        if args.ethernet:
            if args.verbose:
                print(f"Connecting via Ethernet to {args.ethernet}...")
            scope = RigolDHO914S(connection_type='ethernet', 
                               ip_address=args.ethernet, 
                               timeout=args.timeout*1000)
        else:
            if args.verbose:
                print("Connecting via USB...")
            scope = RigolDHO914S(connection_type='usb', timeout=args.timeout*1000)
        
        with scope:
            if args.verbose:
                print(f"Connected to: {scope.get_identity()}")
            
            # Start acquisition if requested
            if args.run_first:
                if args.verbose:
                    print("Starting acquisition...")
                scope.run()
                scope.wait_for_operation_complete(timeout=args.timeout)
            
            # Export data
            print(f"Exporting channels {channels} to {output_dir}")
            print(f"Formats: {', '.join(formats)}")
            
            results = export_multiple_channels(scope, channels, output_dir, formats, args.verbose)
            
            # Display summary
            if args.summary or args.verbose:
                print("\nEXPORT SUMMARY")
                print("=" * 50)
                
                total_files = 0
                total_points = 0
                
                for result in results:
                    if result:
                        print(f"Channel {result['channel']}: {result['points']} points, {len(result['files'])} files")
                        total_files += len(result['files'])
                        total_points += result['points']
                
                print(f"\nTotal: {total_files} files exported, {total_points} data points")
            
            print(f"\n✅ Export completed successfully!")
    
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
