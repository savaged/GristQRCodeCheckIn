namespace GristCheckIn.Core.CheckIn;

/// <summary>Handles the end-to-end "a badge was scanned" workflow.</summary>
public interface ICheckInService
{
    Task<CheckInResult> CheckInAsync(string uuid);
}
