namespace GristCheckIn.Core.CheckIn;

/// <summary>
/// Asks the operator to pick a day when it can't be resolved automatically
/// (e.g. testing outside the event dates). This is the seam that lets the
/// console app prompt via Console.ReadLine while a WPF app can instead pop
/// a dialog or combo box - CheckInService itself never needs to change.
/// </summary>
public interface IDaySelector
{
    string SelectColumn(IReadOnlyDictionary<DayOfWeek, string> options);
}
