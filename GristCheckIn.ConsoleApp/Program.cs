using GristCheckIn.Core.CheckIn;
using GristCheckIn.Core.Configuration;
using GristCheckIn.Core.Grist;
using GristCheckIn.ConsoleApp;

// =========================================================================
// Composition root: this is the ONLY place concrete classes get new'd up
// and wired to their interfaces. Everything downstream of this - the check
// in service, the Grist client - only ever sees the abstractions.
// =========================================================================

var config = GristConfig.FromEnvironment();

// Update these column names to match your actual Grist table.
var dayColumns = new Dictionary<DayOfWeek, string>
{
    [DayOfWeek.Friday] = "Signed_In_Friday",
    [DayOfWeek.Saturday] = "Signed_In_Saturday",
    [DayOfWeek.Sunday] = "Signed_In_Sunday",
};

IGristClient gristClient = new GristClient(config);
IDayColumnResolver dayResolver = new DayColumnResolver(dayColumns);
IDaySelector daySelector = new ConsoleDaySelector();
ICheckInService checkInService = new CheckInService(gristClient, config, dayResolver, daySelector);

Console.WriteLine("=========================================");
Console.WriteLine(" Grist Volunteer Check-In");
Console.WriteLine(" Scan a badge (or Ctrl+C to quit)...");
Console.WriteLine("=========================================");

while (true)
{
    Console.Write("\nScan> ");
    string? scanned = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(scanned))
        continue;

    var result = await checkInService.CheckInAsync(scanned.Trim());
    Print(result);
}

static void Print(CheckInResult result)
{
    switch (result.Status)
    {
        case CheckInStatus.Success:
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Checked in: {result.VolunteerName} ({result.Column})");
            break;

        case CheckInStatus.AlreadyCheckedIn:
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ {result.VolunteerName} was already checked in for {result.Column}.");
            break;

        case CheckInStatus.NotFound:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ {result.Message}");
            break;

        case CheckInStatus.Error:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ Error: {result.Message}");
            break;
    }
    Console.ResetColor();
}
