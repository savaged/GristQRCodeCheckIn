namespace GristCheckIn.Core.CheckIn;

public enum CheckInStatus
{
    Success,
    AlreadyCheckedIn,
    NotFound,
    Error
}

/// <summary>
/// Outcome of a check-in attempt, with no assumptions about how it will be
/// displayed. A console app switches on Status and writes to the terminal;
/// a WPF app could bind Status to an icon/colour and VolunteerName to a label.
/// </summary>
public record CheckInResult(CheckInStatus Status, string? VolunteerName, string? Column, string? Message)
{
    public static CheckInResult Success(string name, string column) =>
        new(CheckInStatus.Success, name, column, null);

    public static CheckInResult AlreadyCheckedIn(string name, string column) =>
        new(CheckInStatus.AlreadyCheckedIn, name, column, null);

    public static CheckInResult NotFound(string uuid) =>
        new(CheckInStatus.NotFound, null, null, $"No volunteer found for UUID '{uuid}'.");

    public static CheckInResult Error(string message) =>
        new(CheckInStatus.Error, null, null, message);
}
