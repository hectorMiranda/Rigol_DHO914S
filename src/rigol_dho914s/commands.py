"""
SCPI command definitions for Rigol DHO914S oscilloscope.
"""

class SCPICommands:
    """Collection of SCPI commands for the Rigol DHO914S."""
    
    # System commands
    IDENTITY = "*IDN?"
    RESET = "*RST"
    CLEAR_STATUS = "*CLS"
    ERROR_QUERY = "SYST:ERR?"
    
    # Display and UI commands
    DISPLAY_ON = "DISP ON"
    DISPLAY_OFF = "DISP OFF"
    DISPLAY_CLEAR = "DISP:CLE"
    
    # Screenshot commands
    SCREENSHOT_SETUP = "DISP:DATA?"
    HARDCOPY_SETUP = "HARD:SOUR DISP"
    HARDCOPY_FORMAT = "HARD:FACT {}"  # PNG, BMP, JPEG
    HARDCOPY_START = "HARD:STAR"
    
    # Channel commands
    CHANNEL_ENABLE = "CHAN{}:DISP {}"  # channel, ON/OFF
    CHANNEL_COUPLING = "CHAN{}:COUP {}"  # channel, AC/DC/GND
    CHANNEL_SCALE = "CHAN{}:SCAL {}"  # channel, scale
    CHANNEL_OFFSET = "CHAN{}:OFFS {}"  # channel, offset
    CHANNEL_PROBE = "CHAN{}:PROB {}"  # channel, probe ratio
    CHANNEL_LABEL = "CHAN{}:LAB {}"  # channel, label
    CHANNEL_BANDWIDTH = "CHAN{}:BAND {}"  # channel, bandwidth limit
    
    # Channel query commands
    CHANNEL_SCALE_QUERY = "CHAN{}:SCAL?"
    CHANNEL_OFFSET_QUERY = "CHAN{}:OFFS?"
    CHANNEL_COUPLING_QUERY = "CHAN{}:COUP?"
    CHANNEL_PROBE_QUERY = "CHAN{}:PROB?"
    
    # Timebase commands
    TIMEBASE_SCALE = "TIM:SCAL {}"
    TIMEBASE_OFFSET = "TIM:OFFS {}"
    TIMEBASE_MODE = "TIM:MODE {}"  # MAIN, ZOOM, XY, ROLL
    
    # Timebase query commands
    TIMEBASE_SCALE_QUERY = "TIM:SCAL?"
    TIMEBASE_OFFSET_QUERY = "TIM:OFFS?"
    TIMEBASE_MODE_QUERY = "TIM:MODE?"
    
    # Trigger commands
    TRIGGER_MODE = "TRIG:MODE {}"  # EDGE, PULSE, RUNT, WIND, NEDG, SLOP, VID, PATT, DEL, TIM, DUR, SHOL, RS232, I2C, SPI
    TRIGGER_SOURCE = "TRIG:EDGE:SOUR {}"
    TRIGGER_LEVEL = "TRIG:EDGE:LEV {}"
    TRIGGER_SLOPE = "TRIG:EDGE:SLOP {}"  # POS, NEG, RFAL
    TRIGGER_COUPLING = "TRIG:COUP {}"  # AC, DC, LFR, HFR
    TRIGGER_STATUS = "TRIG:STAT?"
    TRIGGER_HOLDOFF = "TRIG:HOLD {}"
    
    # Acquisition commands
    ACQUIRE_TYPE = "ACQ:TYPE {}"  # NORM, AVER, PEAK, HRES
    ACQUIRE_MODE = "ACQ:MODE {}"  # RTIM, ETIM
    ACQUIRE_AVERAGE = "ACQ:AVER {}"
    ACQUIRE_MEMDEPTH = "ACQ:MDEP {}"
    
    # Run control commands
    RUN = "RUN"
    STOP = "STOP"
    SINGLE = "SING"
    FORCE_TRIGGER = "TFOR"
    
    # Waveform commands
    WAVEFORM_SOURCE = "WAV:SOUR {}"  # CHAN1, CHAN2, CHAN3, CHAN4, MATH, FFT
    WAVEFORM_MODE = "WAV:MODE {}"  # NORM, MAX, RAW
    WAVEFORM_FORMAT = "WAV:FORM {}"  # WORD, BYTE, ASC
    WAVEFORM_DATA = "WAV:DATA?"
    WAVEFORM_PREAMBLE = "WAV:PRE?"
    WAVEFORM_START = "WAV:STAR {}"
    WAVEFORM_STOP = "WAV:STOP {}"
    
    # Measurement commands
    MEASURE_ITEM = "MEAS:ITEM? {},CHAN{}"  # measurement_type, channel
    MEASURE_CLEAR = "MEAS:CLE"
    MEASURE_GATE_START = "MEAS:GATE:STAR {}"
    MEASURE_GATE_STOP = "MEAS:GATE:STOP {}"
    
    # Math and FFT commands
    MATH_DISPLAY = "MATH:DISP {}"  # ON/OFF
    MATH_OPERATOR = "MATH:OPER {}"
    MATH_SOURCE1 = "MATH:SOUR1 {}"
    MATH_SOURCE2 = "MATH:SOUR2 {}"
    
    # Cursor commands
    CURSOR_FUNCTION = "CURS:FUNC {}"  # OFF, VOLT, TIME, AUTO
    CURSOR_SOURCE = "CURS:SOUR {}"
    CURSOR_MODE = "CURS:MODE {}"  # MANU, TRAC, AUTO
    
    # Save and recall commands
    SAVE_SETUP = "SAV:SET {}"
    RECALL_SETUP = "REC:SET {}"
    SAVE_WAVEFORM = "SAV:WAV:STAR"
    
    # Calibration and self-test
    CALIBRATE = "*CAL?"
    SELF_TEST = "*TST?"


class MeasurementTypes:
    """Available measurement types for the oscilloscope."""
    
    # Voltage measurements
    VOLTAGE_MAX = "VMAX"
    VOLTAGE_MIN = "VMIN"
    VOLTAGE_PP = "VPP"
    VOLTAGE_TOP = "VTOP"
    VOLTAGE_BASE = "VBAS"
    VOLTAGE_AMP = "VAMP"
    VOLTAGE_HIGH = "VHIG"
    VOLTAGE_LOW = "VLOW"
    VOLTAGE_AVERAGE = "VAVE"
    VOLTAGE_RMS = "VRMS"
    VOLTAGE_OVERSHOOT = "OVER"
    VOLTAGE_PRESHOOT = "PRES"
    
    # Time measurements
    PERIOD = "PER"
    FREQUENCY = "FREQ"
    RISE_TIME = "RTIM"
    FALL_TIME = "FTIM"
    PULSE_WIDTH_POSITIVE = "PWID"
    PULSE_WIDTH_NEGATIVE = "NWID"
    DUTY_CYCLE_POSITIVE = "PDUT"
    DUTY_CYCLE_NEGATIVE = "NDUT"
    
    # Digital measurements
    SETUP_TIME = "SETU"
    HOLD_TIME = "HOLD"
    
    # Delay measurements
    DELAY_RISING_RISING = "RDEL"
    DELAY_FALLING_FALLING = "FDEL"
    DELAY_RISING_FALLING = "RFDEL"
    DELAY_FALLING_RISING = "FRDEL"
    
    # Phase measurement
    PHASE = "PHAS"
