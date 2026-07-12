namespace GristCheckIn.Core.Grist;

/// <summary>
/// Grist returns field values as loosely-typed JSON, so booleans come back
/// as JsonElement rather than a plain bool. These helpers centralise that
/// unwrapping so it isn't duplicated wherever fields are read.
/// </summary>
public static class GristFieldHelpers
{
    public static bool IsTrue(object? value) =>
        value is JsonElement element && element.ValueKind == JsonValueKind.True;

    public static string GetString(IReadOnlyDictionary<string, object?> fields, string column, string fallback = "") =>
        fields.TryGetValue(column, out var value) ? value?.ToString() ?? fallback : fallback;
}
