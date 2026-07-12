namespace GristCheckIn.Core.Configuration;

/// <summary>
/// Connection and column-mapping settings for talking to a Grist document.
/// Deliberately just a plain settings bag - it has one job, holding config,
/// and nothing here knows about HTTP, the console, or WPF.
/// </summary>
public partial class GristConfig
{
    /// <summary>e.g. "https://docs.getgrist.com" for Grist SaaS, or your self-hosted URL.</summary>
    public string ServerUrl { get; set; } = "https://docs.getgrist.com";

    /// <summary>Found in the document URL: .../docs/&lt;DOC_ID&gt;/...</summary>
    public string DocId { get; set; } = "PUT_YOUR_DOC_ID_HERE";

    /// <summary>Table name as shown on the "Raw Data" page (not the display name).</summary>
    public string TableId { get; set; } = "Volunteers";

    /// <summary>Account Settings -> API key.</summary>
    public string ApiKey { get; set; } = "PUT_YOUR_API_KEY_HERE";

    /// <summary>Column that stores the UUID encoded in the QR code (Text type).</summary>
    public string UuidColumn { get; set; } = "UUID";

    public string FirstNameColumn { get; set; } = "First_Name";

    public string LastNameColumn { get; set; } = "Last_Name";

    /// <summary>
    /// Builds a config from environment variables, falling back to the
    /// defaults above for anything not set. Useful for the console app;
    /// a WPF app might instead build a GristConfig from appsettings.json
    /// or a settings dialog - either way it's just this same plain class.
    /// </summary>
    public static GristConfig FromEnvironment()
    {
        var config = new GristConfig();

        config.ServerUrl = Environment.GetEnvironmentVariable("GRIST_SERVER_URL") ?? config.ServerUrl;
        config.DocId = Environment.GetEnvironmentVariable("GRIST_DOC_ID") ?? config.DocId;
        config.TableId = Environment.GetEnvironmentVariable("GRIST_TABLE_ID") ?? config.TableId;
        config.ApiKey = Environment.GetEnvironmentVariable("GRIST_API_KEY") ?? config.ApiKey;
        config.UuidColumn = Environment.GetEnvironmentVariable("GRIST_UUID_COLUMN") ?? config.UuidColumn;
        config.FirstNameColumn = Environment.GetEnvironmentVariable("GRIST_FIRSTNAME_COLUMN") ?? config.FirstNameColumn;
        config.LastNameColumn = Environment.GetEnvironmentVariable("GRIST_LASTNAME_COLUMN") ?? config.LastNameColumn;

        return config;
    }
}
