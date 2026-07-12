namespace GristCheckIn.Core.Grist;

/// <summary>
/// Abstraction over the Grist REST API operations this app needs.
/// Kept deliberately narrow (Interface Segregation) rather than a
/// do-everything "IGristService" - this also makes it trivial to
/// substitute a fake/mock implementation in unit tests.
/// </summary>
public interface IGristClient
{
    /// <summary>Looks up a single record by matching the configured UUID column.</summary>
    Task<GristRecord?> FindRecordByUuidAsync(string uuid);

    /// <summary>Sets a single field on a record via PATCH.</summary>
    Task SetFieldAsync(int recordId, string columnName, object value);
}
