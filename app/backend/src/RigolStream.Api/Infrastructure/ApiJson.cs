using System.Text.Json;
using System.Text.Json.Serialization;

namespace RigolStream.Api.Infrastructure;

/// <summary>
/// Shared JSON conventions: camelCase, enums as camelCase strings, nulls omitted.
/// Used both for MVC result serialization and for hand-written SSE frames so the
/// wire format is identical everywhere.
/// </summary>
public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = Build();

    private static JsonSerializerOptions Build()
    {
        var o = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        o.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return o;
    }

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
}
