namespace GristCheckIn.Core.Grist;

/// <summary>A single row read back from a Grist table.</summary>
public class GristRecord
{
    public int Id { get; init; }
    public Dictionary<string, object?> Fields { get; init; } = new();
}
