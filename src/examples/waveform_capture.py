"""
Waveform capture and analysis example for Rigol DHO914S oscilloscope.

This script demonstrates how to capture waveform data, perform analysis,
and save data in various formats.
"""

import sys
import os
import numpy as np
import matplotlib.pyplot as plt
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from rigol_dho914s import RigolDHO914S
from rigol_dho914s.commands import MeasurementTypes


class WaveformAnalyzer:
    """Tool for capturing and analyzing waveforms."""
    
    def __init__(self, connection_type='usb', ip_address=None):
        """Initialize the waveform analyzer."""
        self.connection_type = connection_type
        self.ip_address = ip_address
    
    def capture_single_channel(self, channel=1, filename=None, plot=True):
        """
        Capture waveform from a single channel.
        
        Args:
            channel: Channel number to capture
            filename: Base filename for saved data
            plot: Whether to create a plot
        """
        try:
            with RigolDHO914S(connection_type=self.connection_type,
                            ip_address=self.ip_address) as scope:
                
                print(f"Capturing waveform from Channel {channel}...")
                
                # Ensure channel is enabled
                scope.set_channel_enable(channel, True)
                
                # Start acquisition
                scope.run()
                
                # Wait for trigger
                scope.wait_for_operation_complete()
                
                # Capture waveform data
                waveform = scope.get_waveform_data(channel)
                
                # Get measurements
                voltage_measurements = scope.get_voltage_measurements(channel)
                time_measurements = scope.get_time_measurements(channel)
                
                print(f"Captured {len(waveform['voltage'])} data points")
                
                # Display measurements
                print("\nVoltage Measurements:")
                for name, value in voltage_measurements.items():
                    if value is not None:
                        print(f"  {name}: {value:.6f} V")
                
                print("\nTime Measurements:")
                for name, value in time_measurements.items():
                    if value is not None:
                        if name == 'frequency':
                            print(f"  {name}: {value:.3f} Hz")
                        else:
                            print(f"  {name}: {value:.9f} s")
                
                # Save data if filename provided
                if filename:
                    csv_file = f"{filename}_ch{channel}.csv"
                    scope.save_waveform_csv(waveform, csv_file, channel)
                    print(f"\nWaveform data saved to {csv_file}")
                    
                    # Save measurements
                    measurements_file = f"{filename}_ch{channel}_measurements.txt"
                    self._save_measurements(measurements_file, channel, 
                                          voltage_measurements, time_measurements)
                    print(f"Measurements saved to {measurements_file}")
                
                # Create plot if requested
                if plot:
                    self._plot_waveform(waveform, channel, filename)
                
                return waveform, voltage_measurements, time_measurements
                
        except Exception as e:
            print(f"Error capturing waveform: {e}")
            return None, None, None
    
    def capture_multiple_channels(self, channels=[1, 2], filename=None, plot=True):
        """
        Capture waveforms from multiple channels simultaneously.
        
        Args:
            channels: List of channel numbers to capture
            filename: Base filename for saved data
            plot: Whether to create plots
        """
        try:
            with RigolDHO914S(connection_type=self.connection_type,
                            ip_address=self.ip_address) as scope:
                
                print(f"Capturing waveforms from channels: {channels}")
                
                # Enable specified channels
                for ch in channels:
                    scope.set_channel_enable(ch, True)
                
                # Start acquisition
                scope.run()
                scope.wait_for_operation_complete()
                
                waveforms = {}
                all_measurements = {}
                
                # Capture each channel
                for channel in channels:
                    print(f"\nProcessing Channel {channel}...")
                    
                    waveform = scope.get_waveform_data(channel)
                    voltage_meas = scope.get_voltage_measurements(channel)
                    time_meas = scope.get_time_measurements(channel)
                    
                    waveforms[channel] = waveform
                    all_measurements[channel] = {
                        'voltage': voltage_meas,
                        'time': time_meas
                    }
                    
                    print(f"  Captured {len(waveform['voltage'])} points")
                    
                    # Save individual channel data
                    if filename:
                        csv_file = f"{filename}_ch{channel}.csv"
                        scope.save_waveform_csv(waveform, csv_file, channel)
                        print(f"  Saved to {csv_file}")
                
                # Create combined plot
                if plot:
                    self._plot_multiple_waveforms(waveforms, filename)
                
                # Save combined measurements
                if filename:
                    measurements_file = f"{filename}_all_measurements.txt"
                    self._save_all_measurements(measurements_file, all_measurements)
                
                return waveforms, all_measurements
                
        except Exception as e:
            print(f"Error capturing multiple waveforms: {e}")
            return None, None
    
    def analyze_signal_characteristics(self, channel=1):
        """
        Perform detailed signal analysis.
        
        Args:
            channel: Channel to analyze
        """
        try:
            with RigolDHO914S(connection_type=self.connection_type,
                            ip_address=self.ip_address) as scope:
                
                print(f"Analyzing signal characteristics for Channel {channel}")
                
                # Capture waveform
                waveform = scope.get_waveform_data(channel)
                voltage_data = waveform['voltage']
                time_data = waveform['time']
                
                # Basic statistics
                mean_voltage = np.mean(voltage_data)
                std_voltage = np.std(voltage_data)
                min_voltage = np.min(voltage_data)
                max_voltage = np.max(voltage_data)
                peak_to_peak = max_voltage - min_voltage
                
                print(f"\nStatistical Analysis:")
                print(f"  Mean: {mean_voltage:.6f} V")
                print(f"  Std Dev: {std_voltage:.6f} V")
                print(f"  Min: {min_voltage:.6f} V")
                print(f"  Max: {max_voltage:.6f} V")
                print(f"  Peak-to-Peak: {peak_to_peak:.6f} V")
                
                # FFT analysis
                sample_rate = 1 / (time_data[1] - time_data[0])
                fft_data = np.fft.fft(voltage_data)
                fft_freq = np.fft.fftfreq(len(voltage_data), 1/sample_rate)
                fft_magnitude = np.abs(fft_data)
                
                # Find dominant frequency
                positive_freq_idx = fft_freq > 0
                dominant_freq_idx = np.argmax(fft_magnitude[positive_freq_idx])
                dominant_frequency = fft_freq[positive_freq_idx][dominant_freq_idx]
                
                print(f"\nFrequency Domain Analysis:")
                print(f"  Sample Rate: {sample_rate:.0f} Hz")
                print(f"  Dominant Frequency: {dominant_frequency:.3f} Hz")
                
                # Signal quality metrics
                snr = self._calculate_snr(voltage_data)
                thd = self._calculate_thd(fft_magnitude, dominant_freq_idx)
                
                print(f"  Estimated SNR: {snr:.2f} dB")
                print(f"  Estimated THD: {thd:.4f}%")
                
                return {
                    'statistics': {
                        'mean': mean_voltage,
                        'std': std_voltage,
                        'min': min_voltage,
                        'max': max_voltage,
                        'peak_to_peak': peak_to_peak
                    },
                    'frequency': {
                        'sample_rate': sample_rate,
                        'dominant_frequency': dominant_frequency,
                        'snr': snr,
                        'thd': thd
                    },
                    'fft': {
                        'frequency': fft_freq,
                        'magnitude': fft_magnitude
                    }
                }
                
        except Exception as e:
            print(f"Error in signal analysis: {e}")
            return None
    
    def _plot_waveform(self, waveform, channel, filename=None):
        """Create a plot of the waveform."""
        plt.figure(figsize=(12, 6))
        plt.plot(waveform['time'] * 1000, waveform['voltage'], 'b-', linewidth=1)
        plt.xlabel('Time (ms)')
        plt.ylabel('Voltage (V)')
        plt.title(f'Channel {channel} Waveform')
        plt.grid(True, alpha=0.3)
        
        if filename:
            plot_file = f"{filename}_ch{channel}_plot.png"
            plt.savefig(plot_file, dpi=300, bbox_inches='tight')
            print(f"Plot saved to {plot_file}")
        
        plt.show()
    
    def _plot_multiple_waveforms(self, waveforms, filename=None):
        """Create a plot with multiple waveforms."""
        plt.figure(figsize=(14, 8))
        
        colors = ['blue', 'red', 'green', 'orange']
        
        for i, (channel, waveform) in enumerate(waveforms.items()):
            color = colors[i % len(colors)]
            plt.plot(waveform['time'] * 1000, waveform['voltage'], 
                    color=color, linewidth=1, label=f'Channel {channel}')
        
        plt.xlabel('Time (ms)')
        plt.ylabel('Voltage (V)')
        plt.title('Multi-Channel Waveforms')
        plt.legend()
        plt.grid(True, alpha=0.3)
        
        if filename:
            plot_file = f"{filename}_multichannel_plot.png"
            plt.savefig(plot_file, dpi=300, bbox_inches='tight')
            print(f"Combined plot saved to {plot_file}")
        
        plt.show()
    
    def _save_measurements(self, filename, channel, voltage_meas, time_meas):
        """Save measurements to text file."""
        with open(filename, 'w') as f:
            f.write(f"Rigol DHO914S Channel {channel} Measurements\n")
            f.write(f"Timestamp: {__import__('datetime').datetime.now()}\n")
            f.write("=" * 50 + "\n\n")
            
            f.write("Voltage Measurements:\n")
            for name, value in voltage_meas.items():
                if value is not None:
                    f.write(f"  {name}: {value:.6f} V\n")
            
            f.write("\nTime Measurements:\n")
            for name, value in time_meas.items():
                if value is not None:
                    f.write(f"  {name}: {value:.9f} s\n")
    
    def _save_all_measurements(self, filename, all_measurements):
        """Save measurements from all channels."""
        with open(filename, 'w') as f:
            f.write(f"Rigol DHO914S Multi-Channel Measurements\n")
            f.write(f"Timestamp: {__import__('datetime').datetime.now()}\n")
            f.write("=" * 60 + "\n\n")
            
            for channel, measurements in all_measurements.items():
                f.write(f"CHANNEL {channel}\n")
                f.write("-" * 20 + "\n")
                
                f.write("Voltage Measurements:\n")
                for name, value in measurements['voltage'].items():
                    if value is not None:
                        f.write(f"  {name}: {value:.6f} V\n")
                
                f.write("Time Measurements:\n")
                for name, value in measurements['time'].items():
                    if value is not None:
                        f.write(f"  {name}: {value:.9f} s\n")
                
                f.write("\n")
    
    def _calculate_snr(self, signal):
        """Calculate estimated Signal-to-Noise Ratio."""
        # Simple SNR estimation using signal variance vs noise estimation
        signal_power = np.var(signal)
        # Estimate noise as high-frequency components
        diff_signal = np.diff(signal)
        noise_power = np.var(diff_signal) / 2  # Rough noise estimation
        
        if noise_power > 0:
            snr_linear = signal_power / noise_power
            snr_db = 10 * np.log10(snr_linear)
            return snr_db
        return float('inf')
    
    def _calculate_thd(self, fft_magnitude, fundamental_idx):
        """Calculate Total Harmonic Distortion."""
        # Simple THD calculation
        fundamental = fft_magnitude[fundamental_idx]
        
        # Find harmonics (2nd, 3rd, 4th)
        harmonics = []
        for h in range(2, 5):
            harmonic_idx = h * fundamental_idx
            if harmonic_idx < len(fft_magnitude):
                harmonics.append(fft_magnitude[harmonic_idx])
        
        if fundamental > 0 and harmonics:
            harmonic_power = sum(h**2 for h in harmonics)
            thd = np.sqrt(harmonic_power) / fundamental * 100
            return thd
        return 0.0


def main():
    """Main function for demonstration."""
    analyzer = WaveformAnalyzer()
    
    print("Rigol DHO914S Waveform Capture and Analysis")
    print("=" * 50)
    
    # Single channel capture
    print("\n1. Single Channel Capture:")
    waveform, v_meas, t_meas = analyzer.capture_single_channel(
        channel=1, filename="waveform_capture", plot=True
    )
    
    if waveform:
        # Multi-channel capture
        print("\n2. Multi-Channel Capture:")
        waveforms, all_meas = analyzer.capture_multiple_channels(
            channels=[1, 2], filename="multichannel_capture", plot=True
        )
        
        # Signal analysis
        print("\n3. Signal Analysis:")
        analysis = analyzer.analyze_signal_characteristics(channel=1)


if __name__ == "__main__":
    main()
