"""
Rigol DHO914S Oscilloscope Control Library

A Python library for controlling the Rigol DHO914S oscilloscope via SCPI commands.
"""

from .core import RigolDHO914S
from .exceptions import RigolError, ConnectionError, CommandError

__version__ = "1.0.0"
__author__ = "Hector Miranda"
__email__ = "hector.miranda@example.com"

__all__ = [
    "RigolDHO914S",
    "RigolError", 
    "ConnectionError",
    "CommandError",
]
