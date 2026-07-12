/*
=Option 1=
Set via environment variables...

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

=Option 2=
Use a partial...

partial class GristConfig
{
    public GristConfig()
    {
        ServerUrl = "https://docs.getgrist.com";
        DocId = "PUT_YOUR_DOC_ID_HERE";
        TableId = "GRIST_TABLE_ID";
        ApiKey = "GRIST_API_KEY";
        UuidColumn = "UUID";
        FirstnameColumn = "First Name";
        SurnameColumn = "Last Name";
    }
}
 */
partial class GristConfig
{
    public string ServerUrl { get; set; }
    public string DocId { get; set; }
    public string TableId { get; set; }
    public string ApiKey { get; set; }
    public string UuidColumn { get; set; }
    public string FirstnameColumn { get; set; }
    public string SurnameColumn { get; set; }
}

