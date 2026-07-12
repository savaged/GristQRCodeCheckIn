# Grist Volunteer Check-In

A small .NET solution for checking in convention volunteers by scanning a QR-code badge. Each volunteer's badge encodes a UUID; scanning it looks up their row in a [Grist](https://www.getgrist.com/) table and marks them as checked-in for the current day (Friday / Saturday / Sunday).

## How it works

1. Each volunteer's row in Grist has a UUID and a QR-code column generated from that UUID, printed on their badge.
2. A USB 2D barcode scanner acts like a keyboard: scanning a badge types the UUID followed by Enter.
3. The app looks up the row by UUID via the Grist REST API, checks whether today's column is already `true` (to avoid double-processing a rescan), and if not, sets it to `true`.
4. The operator sees the volunteer's name and a clear success/warning/error message for each scan.

## Project structure

```
GristCheckIn.sln
GristCheckIn.Core/            Reusable class library - no UI dependencies
  Configuration/
    GristConfig.cs            Connection & column-mapping settings
  Grist/
    GristRecord.cs            Plain model for a row read from Grist
    IGristClient.cs           Abstraction over the Grist API
    GristClient.cs            HTTP implementation (GET/PATCH via REST API)
    GristFieldHelpers.cs      Helpers for reading Grist's loosely-typed field values
  CheckIn/
    IDayColumnResolver.cs     Maps a date -> the column for that day
    DayColumnResolver.cs
    IDaySelector.cs           Abstraction for picking a day manually (UI-specific)
    CheckInResult.cs          UI-agnostic outcome of a check-in attempt
    ICheckInService.cs        Orchestrates a single check-in
    CheckInService.cs
GristCheckIn.ConsoleApp/      Console front-end
  ConsoleDaySelector.cs       Console implementation of IDaySelector
  Program.cs                  Composition root + scan loop
```

`GristCheckIn.Core` has no dependency on the console (or any UI framework). It's designed to be referenced from other front-ends — for example a WPF app — by implementing `IDaySelector` for that UI and reusing everything else unchanged.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- A Grist document with:
  - A **Text** column storing a UUID per volunteer (e.g. `UUID`)
  - A column that renders that UUID as a QR code for badge printing
  - First/last name columns for friendly console output
  - Three **Bool** columns for check-in status, e.g. `Signed in Friday`, `Signed in Saturday`, `Signed in Sunday`
- A Grist API key (Account Settings → API)

## Configuration

Settings are read from environment variables via `GristConfig.FromEnvironment()`, falling back to defaults in `GristConfig.cs` if unset.

| Variable | Description | Default |
|---|---|---|
| `GRIST_SERVER_URL` | Grist server base URL (SaaS or self-hosted) | `https://docs.getgrist.com` |
| `GRIST_DOC_ID` | Document ID from the doc's URL | *(placeholder — must be set)* |
| `GRIST_TABLE_ID` | Table name as shown on the "Raw Data" page | `Volunteers` |
| `GRIST_API_KEY` | Your Grist API key | *(placeholder — must be set)* |
| `GRIST_UUID_COLUMN` | Column holding each volunteer's UUID | `UUID` |
| `GRIST_FIRSTNAME_COLUMN` | Column holding first name | `First Name` |
| `GRIST_LASTNAME_COLUMN` | Column holding last name | `Last Name` |

The three day-to-column mappings (currently `Signed in Friday` / `Saturday` / `Sunday`) are set directly in `GristCheckIn.ConsoleApp/Program.cs` — update these to match your table's actual column names.

On Windows (PowerShell):

```powershell
$env:GRIST_DOC_ID = "your-doc-id"
$env:GRIST_API_KEY = "your-api-key"
```

On macOS/Linux:

```bash
export GRIST_DOC_ID="your-doc-id"
export GRIST_API_KEY="your-api-key"
```

## Running

```bash
dotnet build
dotnet run --project GristCheckIn.ConsoleApp
```

Then scan a badge. The app resolves today's check-in column automatically from the current day of week; if run outside Friday–Sunday (e.g. for testing), it will prompt you to pick a day manually.

## Extending

- **Add a UI (e.g. WPF):** reference `GristCheckIn.Core`, implement `IDaySelector` for your UI, and build your own composition root that wires up `GristClient`, `DayColumnResolver`, your `IDaySelector`, and `CheckInService` — no changes needed in Core.
- **Multiple scanning stations:** each station just runs its own instance of the console app (or future UI) pointed at the same document; Grist handles concurrent writes fine at this scale.
- **Logging failed scans:** `CheckInService.CheckInAsync` already returns a `CheckInResult` with a `NotFound`/`Error` status and message — this can be written to a file from the console loop for staff follow-up.

## License

Add your preferred license here.
