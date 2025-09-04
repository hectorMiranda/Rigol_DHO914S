namespace RigolStream.Api.Scpi;

/// <summary>
/// SCPI command strings for the Rigol DHO914S, ported from the Python
/// <c>rigol_dho914s.commands.SCPICommands</c> class so both stacks speak the
/// exact same dialect. Format placeholders use <see cref="string.Format(string, object?[])"/>.
/// </summary>
public static class ScpiCommands
{
    // System
    public const string Identity = "*IDN?";
    public const string Reset = "*RST";
    public const string ClearStatus = "*CLS";
    public const string ErrorQuery = ":SYSTem:ERRor?";
    public const string OperationComplete = "*OPC?";

    // Run control
    public const string Run = ":RUN";
    public const string Stop = ":STOP";
    public const string Single = ":SINGle";
    public const string ForceTrigger = ":TFORce";

    // Channel (args: channel, value)
    public const string ChannelEnable = ":CHANnel{0}:DISPlay {1}";
    public const string ChannelCoupling = ":CHANnel{0}:COUPling {1}";
    public const string ChannelScale = ":CHANnel{0}:SCALe {1}";
    public const string ChannelOffset = ":CHANnel{0}:OFFSet {1}";
    public const string ChannelProbe = ":CHANnel{0}:PROBe {1}";
    public const string ChannelScaleQuery = ":CHANnel{0}:SCALe?";
    public const string ChannelOffsetQuery = ":CHANnel{0}:OFFSet?";
    public const string ChannelCouplingQuery = ":CHANnel{0}:COUPling?";
    public const string ChannelProbeQuery = ":CHANnel{0}:PROBe?";

    // Timebase
    public const string TimebaseScale = ":TIMebase:SCALe {0}";
    public const string TimebaseOffset = ":TIMebase:OFFSet {0}";
    public const string TimebaseScaleQuery = ":TIMebase:SCALe?";
    public const string TimebaseOffsetQuery = ":TIMebase:OFFSet?";

    // Trigger (edge)
    public const string TriggerSource = ":TRIGger:EDGE:SOURce {0}";
    public const string TriggerLevel = ":TRIGger:EDGE:LEVel {0}";
    public const string TriggerSlope = ":TRIGger:EDGE:SLOPe {0}"; // POSitive, NEGative, RFALl
    public const string TriggerStatusQuery = ":TRIGger:STATus?";

    // Waveform
    public const string WaveformSource = ":WAVeform:SOURce {0}";  // CHAN1..CHAN4
    public const string WaveformMode = ":WAVeform:MODE {0}";      // NORMal, MAXimum, RAW
    public const string WaveformFormat = ":WAVeform:FORMat {0}";  // WORD, BYTE, ASCii
    public const string WaveformPoints = ":WAVeform:POINts {0}";
    public const string WaveformData = ":WAVeform:DATA?";
    public const string WaveformPreamble = ":WAVeform:PREamble?";

    // Measurement
    public const string MeasureSource = ":MEASure:SOURce {0}";
    public const string MeasureItem = ":MEASure:ITEM {0}";

    // Display / screenshot
    public const string DisplayData = ":DISPlay:DATA?";
}
