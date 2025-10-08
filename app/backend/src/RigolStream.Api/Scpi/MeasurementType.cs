using RigolStream.Api.Models;

namespace RigolStream.Api.Scpi;

/// <summary>
/// The catalogue of automatic measurements, with the SCPI item code, a friendly
/// name and the physical unit. Ported from the Python <c>MeasurementTypes</c>
/// and enriched with units so the API/UI can label results.
/// </summary>
public sealed record MeasurementType(string Code, string Name, string Unit)
{
    public static readonly MeasurementType Vpp = new("VPP", "Peak-to-peak", "V");
    public static readonly MeasurementType Vmax = new("VMAX", "Maximum", "V");
    public static readonly MeasurementType Vmin = new("VMIN", "Minimum", "V");
    public static readonly MeasurementType Vrms = new("VRMS", "RMS", "V");
    public static readonly MeasurementType Vavg = new("VAVG", "Average", "V");
    public static readonly MeasurementType Vtop = new("VTOP", "Top", "V");
    public static readonly MeasurementType Vbase = new("VBASe", "Base", "V");

    public static readonly MeasurementType Frequency = new("FREQuency", "Frequency", "Hz");
    public static readonly MeasurementType Period = new("PERiod", "Period", "s");
    public static readonly MeasurementType RiseTime = new("RTIMe", "Rise time", "s");
    public static readonly MeasurementType FallTime = new("FTIMe", "Fall time", "s");
    public static readonly MeasurementType PulseWidth = new("PWIDth", "Pos. width", "s");
    public static readonly MeasurementType DutyCycle = new("PDUTy", "Duty cycle", "%");

    /// <summary>Default measurement panel shown for every channel.</summary>
    public static readonly IReadOnlyList<MeasurementType> Default = new[]
    {
        Vpp, Vmax, Vmin, Vrms, Vavg, Frequency, Period, RiseTime, FallTime, DutyCycle,
    };

    /// <summary>The Rigol "no signal / out of range" sentinel value.</summary>
    public const double NoSignalSentinel = 9.9e37;

    public Measurement ToResult(double raw, long timestamp)
    {
        double? value = Math.Abs(raw) >= 1e10 ? null : raw;
        return new Measurement { Name = Name, Code = Code, Value = value, Unit = Unit };
    }
}
