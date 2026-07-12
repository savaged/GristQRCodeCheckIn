var config = new GristConfig();

// Map specific convention dates to their boolean check-in column.
// Update these three dates to match your actual event.
var dayColumns = new Dictionary<DayOfWeek, string>
{
    [DayOfWeek.Friday] = "Signed in Friday",
    [DayOfWeek.Saturday] = "Signed in Saturday",
    [DayOfWeek.Sunday] = "Signed in Sunday",
};
// ==========================================================================

var client = new GristClient(config);

// Work out which column applies today. If today isn't Fri/Sat/Sun
// (e.g. you're testing outside the event), let the operator pick manually.
string checkInColumn = ResolveTodayColumn(dayColumns) ?? PromptForColumn(dayColumns);

Console.WriteLine("=========================================");
Console.WriteLine(" Grist Volunteer Check-In");
Console.WriteLine($" Marking column: {checkInColumn}");
Console.WriteLine(" Scan a badge (or Ctrl+C to quit)...");
Console.WriteLine("=========================================");

while (true)
{
    Console.Write("\nScan> ");
    string? scanned = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(scanned))
        continue;

    string uuid = scanned.Trim();

    try
    {
        var record = await client.FindRecordByUuidAsync(uuid);

        if (record is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ No volunteer found for UUID '{uuid}'. Check the badge / try rescanning.");
            Console.ResetColor();
            continue;
        }

        string name = record.Fields.TryGetValue(config.FirstnameColumn, out var fn) ? fn?.ToString() ?? "(unnamed)" : "(unnamed)";
        name += record.Fields.TryGetValue(config.SurnameColumn, out var sn) ? sn?.ToString() ?? "(unnamed)" : "(unnamed)";

        bool alreadyCheckedIn = record.Fields.TryGetValue(checkInColumn, out var existing)
                                 && existing is JsonElement je && je.ValueKind == JsonValueKind.True;

        if (alreadyCheckedIn)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ {name} was already checked in today. No change made.");
            Console.ResetColor();
            continue;
        }

        await client.SetBoolFieldAsync(record.Id, checkInColumn, true);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Checked in: {name}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ Error: {ex.Message}");
        Console.ResetColor();
    }
}

// -------------------- helper functions (top-level scope) --------------------

static string? ResolveTodayColumn(Dictionary<DayOfWeek, string> map) =>
    map.TryGetValue(DateTime.Now.DayOfWeek, out var col) ? col : null;

static string PromptForColumn(Dictionary<DayOfWeek, string> map)
{
    Console.WriteLine("Today isn't Friday, Saturday or Sunday.");
    var order = new[] { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
    var options = order.Where(map.ContainsKey).Select(d => (Day: d, Column: map[d])).ToList();
    for (int i = 0; i < options.Count; i++)
        Console.WriteLine($"  {i + 1}. {options[i].Day} -> {options[i].Column}");

    while (true)
    {
        Console.Write("Select day number: ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= options.Count)
            return options[choice - 1].Column;
        Console.WriteLine("Invalid choice, try again.");
    }
}

