"""
Custom exceptions for Rigol DHO914S oscilloscope communication.
"""


class RigolError(Exception):
    """Base exception for all Rigol oscilloscope errors."""
    pass


class ConnectionError(RigolError):
    """Raised when there's an issue connecting to the oscilloscope."""
    pass


class CommandError(RigolError):
    """Raised when a SCPI command fails or returns an error."""
    pass


class TimeoutError(RigolError):
    """Raised when a command times out."""
    pass


class DataError(RigolError):
    """Raised when there's an issue with data transfer or parsing."""
    pass
