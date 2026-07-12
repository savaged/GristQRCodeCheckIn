namespace GristCheckIn.Core.CheckIn;

/// <summary>Maps a calendar date to the boolean column that should be set for that day.</summary>
public interface IDayColumnResolver
{
    IReadOnlyDictionary<DayOfWeek, string> DayColumns { get; }

    /// <summary>Returns the column for the given date, or null if that date isn't configured.</summary>
    string? ResolveColumnForDate(DateTime date);
}
