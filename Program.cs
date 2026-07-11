// Grist Volunteer Check-In Scanner
// -----------------------------------------------------------------------
// Reads a volunteer UUID scanned from a QR-code badge (barcode scanners act
// as HID keyboards, so the scanned value + Enter arrives just like typed
// input) and marks that volunteer as checked-in for the current day by
// setting a boolean column via the Grist REST API.
//
// Targets .NET 8. Run with: dotnet run
// -----------------------------------------------------------------------

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// =========================== CONFIGURATION ==============================
// Fill these in for your setup, or set them as environment variables
// (recommended for the API key so it isn't committed to source control).

var config = new GristConfig
{
    // e.g. "https://docs.getgrist.com" for Grist SaaS, or your self-hosted URL
    ServerUrl = Environment.GetEnvironmentVariable("GRIST_SERVER_URL") ?? "https://docs.getgrist.com",

    // Found in the document URL: .../docs/<DOC_ID>/...
    DocId = Environment.GetEnvironmentVariable("GRIST_DOC_ID") ?? "PUT_YOUR_DOC_ID_HERE",

    // Table name as shown on the "Raw Data" page (not the display name)
    TableId = Environment.GetEnvironmentVariable("GRIST_TABLE_ID") ?? "Volunteers",

    // Your personal/API access token: Account Settings -> API key
    ApiKey = Environment.GetEnvironmentVariable("GRIST_API_KEY") ?? "PUT_YOUR_API_KEY_HERE",

    // Column that stores the UUID encoded in the QR code (Text type)
    UuidColumn = "UUID",

    // Column holding the volunteer's firstname, just for friendly console output
    FirstnameColumn = "First Name",

    // Column holding the volunteer's surname, just for friendly console output
    SurnameColumn = "Last Name"
};

// Map specific convention dates to their boolean check-in column.
// Update these three dates to match your actual event.
var dayColumns = new Dictionary<DateOnly, string>
{
    [new DateOnly(2026, 8, 21)] = "CheckedInFri", // Friday
    [new DateOnly(2026, 8, 22)] = "CheckedInSat", // Saturday
    [new DateOnly(2026, 8, 23)] = "CheckedInSun", // Sunday
};
// ==========================================================================

var client = new GristClient(config);

// Work out which column applies today. If today isn't one of the three
// configured dates (e.g. you're testing outside the event), let the
// operator pick manually.
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

static string? ResolveTodayColumn(Dictionary<DateOnly, string> map)
{
    var today = DateOnly.FromDateTime(DateTime.Now);
    return map.TryGetValue(today, out var col) ? col : null;
}

static string PromptForColumn(Dictionary<DateOnly, string> map)
{
    Console.WriteLine("Today doesn't match a configured convention date.");
    var options = map.OrderBy(kv => kv.Key).ToList();
    for (int i = 0; i < options.Count; i++)
        Console.WriteLine($"  {i + 1}. {options[i].Key:ddd dd MMM} -> {options[i].Value}");

    while (true)
    {
        Console.Write("Select day number: ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= options.Count)
            return options[choice - 1].Value;
        Console.WriteLine("Invalid choice, try again.");
    }
}

// =============================== Grist API client ===============================

class GristConfig
{
    public required string ServerUrl { get; init; }
    public required string DocId { get; init; }
    public required string TableId { get; init; }
    public required string ApiKey { get; init; }
    public required string UuidColumn { get; init; }
    public required string FirstnameColumn { get; init; }
    public required string SurnameColumn { get; init; }
}

class GristRecord
{
    public int Id { get; init; }
    public Dictionary<string, object?> Fields { get; init; } = new();
}

class GristClient
{
    private readonly HttpClient _http;
    private readonly GristConfig _config;

    public GristClient(GristConfig config)
    {
        _config = config;
        _http = new HttpClient { BaseAddress = new Uri(config.ServerUrl) };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
    }

    /// <summary>
    /// Looks up a single record by matching the UUID column.
    /// Uses Grist's ?filter= query param, which takes a JSON object
    /// mapping column name -> array of accepted values.
    /// </summary>
    public async Task<GristRecord?> FindRecordByUuidAsync(string uuid)
    {
        var filter = JsonSerializer.Serialize(new Dictionary<string, string[]>
        {
            [_config.UuidColumn] = new[] { uuid }
        });

        string url = $"/api/docs/{_config.DocId}/tables/{_config.TableId}/records?filter={Uri.EscapeDataString(filter)}";

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RecordsResponse>();

        var match = payload?.Records?.FirstOrDefault();
        if (match is null) return null;

        return new GristRecord { Id = match.Id, Fields = match.Fields ?? new() };
    }

    /// <summary>
    /// Sets a single boolean field on a record via PATCH.
    /// </summary>
    public async Task SetBoolFieldAsync(int recordId, string columnName, bool value)
    {
        var body = new
        {
            records = new[]
            {
                new
                {
                    id = recordId,
                    fields = new Dictionary<string, object> { [columnName] = value }
                }
            }
        };

        string url = $"/api/docs/{_config.DocId}/tables/{_config.TableId}/records";

        var response = await _http.PatchAsync(url, JsonContent.Create(body));

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Grist API returned {(int)response.StatusCode}: {error}");
        }
    }

    // --- DTOs matching Grist's /records response shape ---

    class RecordsResponse
    {
        [JsonPropertyName("records")]
        public List<RecordDto>? Records { get; set; }
    }

    class RecordDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, object?>? Fields { get; set; }
    }
}
