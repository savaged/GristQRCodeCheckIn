namespace GristCheckIn.Core.CheckIn;

/// <summary>
/// Resolves the check-in column purely from day-of-week (Fri/Sat/Sun),
/// so it keeps working year to year without hardcoded dates. The mapping
/// itself is passed in rather than hardcoded here, so the same class works
/// for any event's actual column names (Open/Closed - no need to modify
/// this class if the columns are renamed).
/// </summary>
public class DayColumnResolver : IDayColumnResolver
{
    public IReadOnlyDictionary<DayOfWeek, string> DayColumns { get; }

    public DayColumnResolver(IReadOnlyDictionary<DayOfWeek, string> dayColumns)
    {
        DayColumns = dayColumns;
    }

    public string? ResolveColumnForDate(DateTime date) =>
        DayColumns.TryGetValue(date.DayOfWeek, out var column) ? column : null;
}
