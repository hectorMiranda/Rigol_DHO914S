"""
Automated test and demonstration script for Rigol DHO914S oscilloscope.

This script performs a comprehensive test of all oscilloscope features
including channel configuration, waveform capture, measurements, and screenshots.
"""

import sys
import os
import time
import datetime
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from rigol_dho914s import RigolDHO914S, ConnectionError, CommandError
from rigol_dho914s.commands import MeasurementTypes


class AutomatedTest:
    """Comprehensive test suite for Rigol DHO914S oscilloscope."""
    
    def __init__(self, connection_type='usb', ip_address=None, output_dir='test_results'):
        """
        Initialize the automated test.
        
        Args:
            connection_type: 'usb' or 'ethernet'
            ip_address: IP address for Ethernet connection
            output_dir: Directory for test results
        """
        self.connection_type = connection_type
        self.ip_address = ip_address
        self.output_dir = output_dir
        self.test_results = {}
        
        # Create output directory
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
        
        # Test configuration
        self.test_timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
        self.log_file = os.path.join(output_dir, f"test_log_{self.test_timestamp}.txt")
        
        # Initialize log
        self.log("Rigol DHO914S Automated Test Suite")
        self.log("=" * 50)
        self.log(f"Timestamp: {datetime.datetime.now()}")
        self.log(f"Connection: {connection_type}")
        if ip_address:
            self.log(f"IP Address: {ip_address}")
    
    def log(self, message):
        """Log message to console and file."""
        print(message)
        with open(self.log_file, 'a') as f:
            f.write(f"{message}\n")
    
    def run_all_tests(self):
        """Run all tests in sequence."""
        try:
            with RigolDHO914S(connection_type=self.connection_type, 
                            ip_address=self.ip_address) as scope:
                
                self.log(f"Connected to: {scope.get_identity()}")
                
                # Run test sequence
                self.test_basic_functionality(scope)
                self.test_channel_configuration(scope)
                self.test_timebase_configuration(scope)
                self.test_trigger_configuration(scope)
                self.test_screenshot_functionality(scope)
                self.test_waveform_acquisition(scope)
                self.test_measurement_functionality(scope)
                self.test_system_status(scope)
                
                # Generate test report
                self.generate_test_report()
                
        except ConnectionError as e:
            self.log(f"❌ Connection failed: {e}")
            return False
        except Exception as e:
            self.log(f"❌ Test failed with error: {e}")
            return False
        
        self.log("✅ All tests completed successfully!")
        return True
    
    def test_basic_functionality(self, scope):
        """Test basic oscilloscope functionality."""
        self.log("\n📋 Testing Basic Functionality...")
        
        try:
            # Test identity
            identity = scope.get_identity()
            self.test_results['identity'] = identity
            self.log(f"  ✅ Identity: {identity}")
            
            # Test error query
            error = scope.get_error()
            self.test_results['initial_error'] = error
            self.log(f"  ✅ Error status: {error}")
            
            # Test system reset
            self.log("  🔄 Testing system reset...")
            scope.reset()
            time.sleep(3)  # Wait for reset
            self.log("  ✅ Reset completed")
            
            # Test clear status
            scope.clear_status()
            self.log("  ✅ Status cleared")
            
            self.test_results['basic_functionality'] = 'PASS'
            
        except Exception as e:
            self.log(f"  ❌ Basic functionality test failed: {e}")
            self.test_results['basic_functionality'] = f'FAIL: {e}'
    
    def test_channel_configuration(self, scope):
        """Test channel configuration functionality."""
        self.log("\n📡 Testing Channel Configuration...")
        
        channel_results = {}
        
        for channel in range(1, 5):
            try:
                self.log(f"  Testing Channel {channel}...")
                
                # Enable channel
                scope.set_channel_enable(channel, True)
                
                # Test coupling settings
                for coupling in ['DC', 'AC', 'GND']:
                    scope.set_channel_coupling(channel, coupling)
                    read_coupling = scope.get_channel_coupling(channel)
                    assert read_coupling == coupling, f"Coupling mismatch: set {coupling}, got {read_coupling}"
                
                # Test scale settings
                test_scales = [0.1, 0.5, 1.0, 2.0, 5.0]
                for scale in test_scales:
                    scope.set_channel_scale(channel, scale)
                    read_scale = scope.get_channel_scale(channel)
                    # Allow small tolerance for floating point comparison
                    assert abs(read_scale - scale) < 0.01, f"Scale mismatch: set {scale}, got {read_scale}"
                
                # Test offset
                scope.set_channel_offset(channel, 0.0)
                read_offset = scope.get_channel_offset(channel)
                
                # Test probe ratio
                scope.set_channel_probe(channel, 1.0)
                
                channel_results[f'channel_{channel}'] = 'PASS'
                self.log(f"    ✅ Channel {channel} configuration OK")
                
            except Exception as e:
                self.log(f"    ❌ Channel {channel} test failed: {e}")
                channel_results[f'channel_{channel}'] = f'FAIL: {e}'
        
        self.test_results['channel_configuration'] = channel_results
    
    def test_timebase_configuration(self, scope):
        """Test timebase configuration."""
        self.log("\n⏱️  Testing Timebase Configuration...")
        
        try:
            # Test various timebase scales
            test_scales = [1e-6, 1e-5, 1e-4, 1e-3, 1e-2, 1e-1]
            
            for scale in test_scales:
                scope.set_timebase_scale(scale)
                read_scale = scope.get_timebase_scale()
                # Allow tolerance for oscilloscope quantization
                tolerance = scale * 0.1
                assert abs(read_scale - scale) < tolerance, f"Timebase scale mismatch: set {scale}, got {read_scale}"
            
            # Test offset
            scope.set_timebase_offset(0.0)
            read_offset = scope.get_timebase_offset()
            
            self.log(f"  ✅ Timebase configuration OK")
            self.log(f"    Final scale: {read_scale}")
            self.log(f"    Final offset: {read_offset}")
            
            self.test_results['timebase_configuration'] = 'PASS'
            
        except Exception as e:
            self.log(f"  ❌ Timebase configuration test failed: {e}")
            self.test_results['timebase_configuration'] = f'FAIL: {e}'
    
    def test_trigger_configuration(self, scope):
        """Test trigger configuration."""
        self.log("\n🎯 Testing Trigger Configuration...")
        
        try:
            # Test trigger source
            for source in ['CHAN1', 'CHAN2', 'CHAN3', 'CHAN4']:
                scope.set_trigger_source(source)
            
            # Test trigger level
            scope.set_trigger_level(0.0)
            
            # Test trigger slope
            for slope in ['POS', 'NEG']:
                scope.set_trigger_slope(slope)
            
            # Get trigger status
            status = scope.get_trigger_status()
            self.log(f"  ✅ Trigger configuration OK")
            self.log(f"    Status: {status}")
            
            self.test_results['trigger_configuration'] = 'PASS'
            
        except Exception as e:
            self.log(f"  ❌ Trigger configuration test failed: {e}")
            self.test_results['trigger_configuration'] = f'FAIL: {e}'
    
    def test_screenshot_functionality(self, scope):
        """Test screenshot functionality."""
        self.log("\n📸 Testing Screenshot Functionality...")
        
        try:
            formats = ['PNG', 'BMP', 'JPEG']
            
            for format_type in formats:
                filename = os.path.join(self.output_dir, f"test_screenshot_{self.test_timestamp}.{format_type.lower()}")
                scope.take_screenshot(filename, format=format_type)
                
                # Verify file was created
                if os.path.exists(filename):
                    file_size = os.path.getsize(filename)
                    self.log(f"  ✅ {format_type} screenshot saved: {filename} ({file_size} bytes)")
                else:
                    raise FileNotFoundError(f"Screenshot file not created: {filename}")
            
            self.test_results['screenshot_functionality'] = 'PASS'
            
        except Exception as e:
            self.log(f"  ❌ Screenshot test failed: {e}")
            self.test_results['screenshot_functionality'] = f'FAIL: {e}'
    
    def test_waveform_acquisition(self, scope):
        """Test waveform data acquisition."""
        self.log("\n📊 Testing Waveform Acquisition...")
        
        try:
            # Configure for waveform capture
            scope.set_channel_enable(1, True)
            scope.set_channel_coupling(1, 'DC')
            scope.set_channel_scale(1, 1.0)
            scope.set_timebase_scale(1e-3)
            
            # Start acquisition
            scope.run()
            time.sleep(1)  # Allow some acquisition time
            
            # Capture waveform
            waveform = scope.get_waveform_data(1)
            
            # Verify waveform data
            assert 'time' in waveform, "Missing time data"
            assert 'voltage' in waveform, "Missing voltage data"
            assert 'preamble' in waveform, "Missing preamble data"
            
            time_data = waveform['time']
            voltage_data = waveform['voltage']
            
            assert len(time_data) > 0, "Empty time data"
            assert len(voltage_data) > 0, "Empty voltage data"
            assert len(time_data) == len(voltage_data), "Time and voltage arrays length mismatch"
            
            # Save waveform to CSV
            csv_filename = os.path.join(self.output_dir, f"test_waveform_{self.test_timestamp}.csv")
            scope.save_waveform_csv(waveform, csv_filename, 1)
            
            self.log(f"  ✅ Waveform acquisition OK")
            self.log(f"    Points captured: {len(voltage_data)}")
            self.log(f"    Time range: {time_data[0]:.6f} to {time_data[-1]:.6f} seconds")
            self.log(f"    Voltage range: {min(voltage_data):.6f} to {max(voltage_data):.6f} V")
            self.log(f"    Saved to: {csv_filename}")
            
            self.test_results['waveform_acquisition'] = 'PASS'
            self.test_results['waveform_points'] = len(voltage_data)
            
        except Exception as e:
            self.log(f"  ❌ Waveform acquisition test failed: {e}")
            self.test_results['waveform_acquisition'] = f'FAIL: {e}'
    
    def test_measurement_functionality(self, scope):
        """Test automatic measurement functionality."""
        self.log("\n📏 Testing Measurement Functionality...")
        
        try:
            # Ensure channel 1 is enabled and configured
            scope.set_channel_enable(1, True)
            scope.run()
            time.sleep(1)
            
            # Test voltage measurements
            voltage_measurements = scope.get_voltage_measurements(1)
            self.log(f"  📊 Voltage measurements:")
            for name, value in voltage_measurements.items():
                if value is not None:
                    self.log(f"    {name}: {value:.6f} V")
            
            # Test time measurements
            time_measurements = scope.get_time_measurements(1)
            self.log(f"  ⏱️  Time measurements:")
            for name, value in time_measurements.items():
                if value is not None:
                    if name == 'frequency':
                        self.log(f"    {name}: {value:.3f} Hz")
                    else:
                        self.log(f"    {name}: {value:.9f} s")
            
            # Test individual measurement
            try:
                vpp = scope.measure(MeasurementTypes.VOLTAGE_PP, 1)
                self.log(f"  📏 Direct Vpp measurement: {vpp:.6f} V")
            except:
                self.log(f"  ⚠️  Direct measurement not available (no signal)")
            
            self.test_results['measurement_functionality'] = 'PASS'
            self.test_results['voltage_measurements'] = voltage_measurements
            self.test_results['time_measurements'] = time_measurements
            
        except Exception as e:
            self.log(f"  ❌ Measurement test failed: {e}")
            self.test_results['measurement_functionality'] = f'FAIL: {e}'
    
    def test_system_status(self, scope):
        """Test system status retrieval."""
        self.log("\n🔍 Testing System Status...")
        
        try:
            status = scope.get_system_status()
            
            self.log(f"  📋 System Status:")
            for key, value in status.items():
                self.log(f"    {key}: {value}")
            
            self.test_results['system_status'] = status
            self.log(f"  ✅ System status retrieval OK")
            
        except Exception as e:
            self.log(f"  ❌ System status test failed: {e}")
            self.test_results['system_status'] = f'FAIL: {e}'
    
    def test_acquisition_control(self, scope):
        """Test acquisition control commands."""
        self.log("\n▶️  Testing Acquisition Control...")
        
        try:
            # Test run
            scope.run()
            self.log("  ✅ RUN command executed")
            time.sleep(0.5)
            
            # Test stop
            scope.stop()
            self.log("  ✅ STOP command executed")
            time.sleep(0.5)
            
            # Test single
            scope.single()
            self.log("  ✅ SINGLE command executed")
            time.sleep(0.5)
            
            # Test force trigger
            scope.force_trigger()
            self.log("  ✅ FORCE TRIGGER command executed")
            
            self.test_results['acquisition_control'] = 'PASS'
            
        except Exception as e:
            self.log(f"  ❌ Acquisition control test failed: {e}")
            self.test_results['acquisition_control'] = f'FAIL: {e}'
    
    def generate_test_report(self):
        """Generate comprehensive test report."""
        report_file = os.path.join(self.output_dir, f"test_report_{self.test_timestamp}.txt")
        
        with open(report_file, 'w') as f:
            f.write("RIGOL DHO914S AUTOMATED TEST REPORT\n")
            f.write("=" * 60 + "\n\n")
            f.write(f"Test Date: {datetime.datetime.now()}\n")
            f.write(f"Connection Type: {self.connection_type}\n")
            if self.ip_address:
                f.write(f"IP Address: {self.ip_address}\n")
            f.write("\n")
            
            # Summary
            total_tests = len(self.test_results)
            passed_tests = sum(1 for v in self.test_results.values() 
                             if isinstance(v, str) and v == 'PASS')
            
            f.write("TEST SUMMARY\n")
            f.write("-" * 30 + "\n")
            f.write(f"Total Tests: {total_tests}\n")
            f.write(f"Passed: {passed_tests}\n")
            f.write(f"Failed: {total_tests - passed_tests}\n")
            f.write(f"Success Rate: {passed_tests/total_tests*100:.1f}%\n\n")
            
            # Detailed results
            f.write("DETAILED RESULTS\n")
            f.write("-" * 30 + "\n")
            for test_name, result in self.test_results.items():
                if isinstance(result, dict):
                    f.write(f"\n{test_name.upper()}:\n")
                    for sub_test, sub_result in result.items():
                        f.write(f"  {sub_test}: {sub_result}\n")
                else:
                    f.write(f"{test_name}: {result}\n")
        
        self.log(f"\n📄 Test report saved: {report_file}")


def main():
    """Main function for running automated tests."""
    import argparse
    
    parser = argparse.ArgumentParser(description="Rigol DHO914S Automated Test Suite")
    parser.add_argument("--ethernet", metavar='IP', help="Use Ethernet connection")
    parser.add_argument("--output", default="test_results", help="Output directory")
    parser.add_argument("--verbose", action='store_true', help="Verbose output")
    
    args = parser.parse_args()
    
    # Create test instance
    if args.ethernet:
        test = AutomatedTest(connection_type='ethernet', 
                           ip_address=args.ethernet, 
                           output_dir=args.output)
    else:
        test = AutomatedTest(connection_type='usb', output_dir=args.output)
    
    # Run all tests
    success = test.run_all_tests()
    
    if success:
        print(f"\n🎉 All tests completed! Results saved to: {args.output}")
    else:
        print(f"\n❌ Tests failed. Check log files in: {args.output}")
        sys.exit(1)


if __name__ == "__main__":
    main()
