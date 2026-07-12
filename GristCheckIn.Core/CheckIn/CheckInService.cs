using GristCheckIn.Core.Configuration;
using GristCheckIn.Core.Grist;

namespace GristCheckIn.Core.CheckIn;

/// <summary>
/// Coordinates a single check-in: work out today's column, look the
/// volunteer up, guard against double check-ins, and write the update.
///
/// Notice this class depends only on interfaces (IGristClient,
/// IDayColumnResolver, IDaySelector) plus the plain GristConfig settings
/// object - never on GristClient, Console, or anything UI-specific. That's
/// what lets it be reused unchanged from a console app, a WPF app, or a
/// unit test with fake implementations of those interfaces.
/// </summary>
public class CheckInService : ICheckInService
{
    private readonly IGristClient _client;
    private readonly GristConfig _config;
    private readonly IDayColumnResolver _dayResolver;
    private readonly IDaySelector _daySelector;

    public CheckInService(
        IGristClient client,
        GristConfig config,
        IDayColumnResolver dayResolver,
        IDaySelector daySelector)
    {
        _client = client;
        _config = config;
        _dayResolver = dayResolver;
        _daySelector = daySelector;
    }

    public async Task<CheckInResult> CheckInAsync(string uuid)
    {
        try
        {
            string column = _dayResolver.ResolveColumnForDate(DateTime.Now)
                             ?? _daySelector.SelectColumn(_dayResolver.DayColumns);

            var record = await _client.FindRecordByUuidAsync(uuid);
            if (record is null)
                return CheckInResult.NotFound(uuid);

            string name = BuildDisplayName(record);

            record.Fields.TryGetValue(column, out var existingValue);
            if (GristFieldHelpers.IsTrue(existingValue))
                return CheckInResult.AlreadyCheckedIn(name, column);

            await _client.SetFieldAsync(record.Id, column, true);
            return CheckInResult.Success(name, column);
        }
        catch (Exception ex)
        {
            return CheckInResult.Error(ex.Message);
        }
    }

    private string BuildDisplayName(GristRecord record)
    {
        string first = GristFieldHelpers.GetString(record.Fields, _config.FirstNameColumn);
        string last = GristFieldHelpers.GetString(record.Fields, _config.LastNameColumn);
        string name = $"{first} {last}".Trim();
        return string.IsNullOrEmpty(name) ? "(unnamed)" : name;
    }
}
