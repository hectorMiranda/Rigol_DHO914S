namespace RigolStream.Api.Models;

/// <summary>
/// Identity of the connected instrument, parsed from the SCPI <c>*IDN?</c>
/// response (<c>manufacturer,model,serial,firmware</c>).
/// </summary>
public sealed record DeviceInfo
{
    public required string Manufacturer { get; init; }
    public required string Model { get; init; }
    public required string SerialNumber { get; init; }
    public required string FirmwareVersion { get; init; }

    /// <summary>True when the API is serving the built-in signal simulator.</summary>
    public bool Simulated { get; init; }

    /// <summary>The raw <c>*IDN?</c> string, useful for debugging.</summary>
    public string? RawIdentity { get; init; }

    /// <summary>
    /// Parse a comma-separated <c>*IDN?</c> response. Missing fields degrade to
    /// empty strings rather than throwing, since firmware revisions vary.
    /// </summary>
    public static DeviceInfo Parse(string identity, bool simulated = false)
    {
        var parts = (identity ?? string.Empty).Split(',', StringSplitOptions.TrimEntries);
        return new DeviceInfo
        {
            Manufacturer = parts.ElementAtOrDefault(0) ?? string.Empty,
            Model = parts.ElementAtOrDefault(1) ?? string.Empty,
            SerialNumber = parts.ElementAtOrDefault(2) ?? string.Empty,
            FirmwareVersion = parts.ElementAtOrDefault(3) ?? string.Empty,
            Simulated = simulated,
            RawIdentity = identity,
        };
    }
}
