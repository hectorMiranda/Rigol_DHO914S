"""
Test connection functionality for Rigol DHO914S oscilloscope.
"""

import sys
import os
import unittest
from unittest.mock import Mock, patch

# Add the src directory to the path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from rigol_dho914s import RigolDHO914S, ConnectionError


class TestConnection(unittest.TestCase):
    """Test oscilloscope connection functionality."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.mock_instrument = Mock()
        self.mock_resource_manager = Mock()
    
    @patch('rigol_dho914s.core.pyvisa')
    def test_usb_connection_success(self, mock_pyvisa):
        """Test successful USB connection."""
        # Mock PyVISA components
        mock_pyvisa.ResourceManager.return_value = self.mock_resource_manager
        self.mock_resource_manager.open_resource.return_value = self.mock_instrument
        self.mock_instrument.query.return_value = "RIGOL TECHNOLOGIES,DHO914S,ABC123,01.01.00"
        
        # Mock find_usb_devices
        with patch('rigol_dho914s.core.find_usb_devices', return_value=['USB::0x1AB1::0x04CE::DHO9XXXX::INSTR']):
            scope = RigolDHO914S(connection_type='usb')
            
            # Verify connection was established
            self.mock_resource_manager.open_resource.assert_called()
            self.mock_instrument.query.assert_called_with("*IDN?")
            
            scope.close()
    
    @patch('rigol_dho914s.core.pyvisa')
    def test_ethernet_connection_success(self, mock_pyvisa):
        """Test successful Ethernet connection."""
        # Mock PyVISA components
        mock_pyvisa.ResourceManager.return_value = self.mock_resource_manager
        self.mock_resource_manager.open_resource.return_value = self.mock_instrument
        self.mock_instrument.query.return_value = "RIGOL TECHNOLOGIES,DHO914S,ABC123,01.01.00"
        
        scope = RigolDHO914S(connection_type='ethernet', ip_address='192.168.1.100')
        
        # Verify connection was established with correct resource string
        self.mock_resource_manager.open_resource.assert_called_with('TCPIP::192.168.1.100::INSTR')
        self.mock_instrument.query.assert_called_with("*IDN?")
        
        scope.close()
    
    @patch('rigol_dho914s.core.pyvisa')
    def test_connection_failure_no_device(self, mock_pyvisa):
        """Test connection failure when no device is found."""
        # Mock find_usb_devices to return empty list
        with patch('rigol_dho914s.core.find_usb_devices', return_value=[]):
            with self.assertRaises(ConnectionError):
                RigolDHO914S(connection_type='usb')
    
    @patch('rigol_dho914s.core.pyvisa')
    def test_connection_failure_wrong_device(self, mock_pyvisa):
        """Test connection failure when connected device is not DHO914S."""
        # Mock PyVISA components
        mock_pyvisa.ResourceManager.return_value = self.mock_resource_manager
        self.mock_resource_manager.open_resource.return_value = self.mock_instrument
        self.mock_instrument.query.return_value = "RIGOL TECHNOLOGIES,DHO804,ABC123,01.01.00"
        
        with patch('rigol_dho914s.core.find_usb_devices', return_value=['USB::0x1AB1::0x04CE::DHO8XX::INSTR']):
            with self.assertRaises(ConnectionError):
                RigolDHO914S(connection_type='usb')
    
    def test_invalid_connection_type(self):
        """Test invalid connection type raises error."""
        with self.assertRaises(ValueError):
            RigolDHO914S(connection_type='invalid')
    
    def test_ethernet_without_ip(self):
        """Test Ethernet connection without IP address raises error."""
        with self.assertRaises(ValueError):
            RigolDHO914S(connection_type='ethernet')
    
    @patch('rigol_dho914s.core.pyvisa')
    def test_context_manager(self, mock_pyvisa):
        """Test context manager functionality."""
        # Mock PyVISA components
        mock_pyvisa.ResourceManager.return_value = self.mock_resource_manager
        self.mock_resource_manager.open_resource.return_value = self.mock_instrument
        self.mock_instrument.query.return_value = "RIGOL TECHNOLOGIES,DHO914S,ABC123,01.01.00"
        
        with patch('rigol_dho914s.core.find_usb_devices', return_value=['USB::0x1AB1::0x04CE::DHO9XXXX::INSTR']):
            with RigolDHO914S(connection_type='usb') as scope:
                self.assertIsNotNone(scope)
            
            # Verify close was called
            self.mock_instrument.close.assert_called()
            self.mock_resource_manager.close.assert_called()


class TestConnectionUtilities(unittest.TestCase):
    """Test connection utility functions."""
    
    @patch('rigol_dho914s.utils.pyvisa')
    def test_find_usb_devices(self, mock_pyvisa):
        """Test USB device discovery."""
        from rigol_dho914s.utils import find_usb_devices
        
        # Mock ResourceManager
        mock_rm = Mock()
        mock_pyvisa.ResourceManager.return_value = mock_rm
        mock_rm.list_resources.return_value = [
            'USB::0x1AB1::0x04CE::DHO9XXXX::INSTR',
            'USB::0x0957::0x1234::OTHER_DEVICE::INSTR',
            'TCPIP::192.168.1.100::INSTR',
            'USB::0x1AB1::0x04CE::DHO8XX::INSTR'
        ]
        
        devices = find_usb_devices()
        
        # Should find only Rigol devices
        expected_devices = [
            'USB::0x1AB1::0x04CE::DHO9XXXX::INSTR',
            'USB::0x1AB1::0x04CE::DHO8XX::INSTR'
        ]
        self.assertEqual(len(devices), 2)
        for device in expected_devices:
            self.assertIn(device, devices)


if __name__ == '__main__':
    unittest.main()
