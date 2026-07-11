Here's a C# console app that does exactly this: reads a UUID from the barcode scanner (which behaves like a keyboard), looks up the matching row in your Grist table via the API, and flips the correct day's boolean column to true.
A few assumptions I've made — let me know if any need adjusting:

Your table has a text column (e.g. UUID) holding the same UUID that's encoded in the QR code
You have three bool columns, e.g. CheckedInFri, CheckedInSat, CheckedInSun
One laptop/PC will run this at the check-in desk with the scanner plugged in as a USB "keyboard" device (scan → text appears → Enter is sent automatically — this is true of virtually all 2D barcode scanners)
Created 2 files, read a fileCreated 2 files, read a fileHow it works

The scanner types the badge's UUID + Enter into the console, just like a keyboard.
The app calls Grist's GET /records?filter=... to find the row whose UUID column matches, using Grist's REST API filter syntax.
It checks whether today's bool column is already true (avoids double-processing a re-scan) and otherwise PATCHes that one field to true.
It prints the volunteer's name so the person on the desk gets instant visual confirmation.

Before running it, you'll need to:

Set three environment variables (or edit the constants directly): GRIST_SERVER_URL, GRIST_DOC_ID, GRIST_API_KEY. The doc ID is the string in your document's URL; the API key comes from Account Settings → API in Grist.
Confirm the TableId and UuidColumn/NameColumn names match your table exactly (use the "Raw Data" page in Grist to see the real column IDs, which may differ from the display labels).
Update the three dates in dayColumns to your actual Fri/Sat/Sun convention dates, and make sure CheckedInFri/CheckedInSat/CheckedInSun match your real Bool column names.
Run dotnet restore && dotnet run from the GristCheckIn folder.

A couple of practical notes for the event:

If you'll have more than one scanning station, each just needs this same app pointed at the same doc — Grist handles concurrent writes fine for this volume.
Right now a failed lookup (bad/unregistered UUID) just prints an error and loops — you may want to log rejected scans to a file so staff can follow up on anyone whose badge didn't work.
If you want a running total of check-ins per day printed at the desk, that's an easy addition (just tally successful check-ins in the loop) — happy to add it if useful.