using GristCheckIn.Core.CheckIn;
using GristCheckIn.Core.Configuration;
using GristCheckIn.Core.DependencyInjection;
using GristCheckIn.ConsoleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// =========================================================================
// Composition root: this is the ONLY place concrete classes get wired to
// their interfaces. Everything downstream of this - the check-in service,
// the Grist client - only ever sees the abstractions, resolved via DI.
// =========================================================================

var config = GristConfig.FromEnvironment();

// Update these column names to match your actual Grist table.
var dayColumns = new Dictionary<DayOfWeek, string>
{
    [DayOfWeek.Friday] = "Signed_In_Friday",
    [DayOfWeek.Saturday] = "Signed_In_Saturday",
    [DayOfWeek.Sunday] = "Signed_In_Sunday",
};

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // The default host logs each HttpClient call at Information level,
        // which would clutter the scan prompt below - keep it to warnings+.
        logging.SetMinimumLevel(LogLevel.Warning);
    })
    .ConfigureServices(services =>
    {
        services.AddGristCheckIn(config, dayColumns);
        services.AddSingleton<IDaySelector, ConsoleDaySelector>();
    })
    .Build();

var checkInService = host.Services.GetRequiredService<ICheckInService>();

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
