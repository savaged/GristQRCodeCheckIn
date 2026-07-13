using GristCheckIn.Core.Configuration;

namespace GristCheckIn.Core.Grist;

/// <summary>
/// Talks to the Grist REST API. This class's only job is translating
/// between HTTP/JSON and the plain GristRecord model - it doesn't know
/// anything about check-in days, volunteer names, or how it's presented.
///
/// Registered as a typed client via AddHttpClient (see ServiceCollectionExtensions),
/// so the HttpClient itself - including its BaseAddress and auth header - is
/// created and configured by IHttpClientFactory, not by this class. That avoids
/// the socket-exhaustion problems that come from a class newing up its own
/// long-lived HttpClient, and lets DI manage its lifetime.
/// </summary>
public class GristClient : IGristClient
{
    private readonly HttpClient _http;
    private readonly GristConfig _config;

    public GristClient(HttpClient httpClient, GristConfig config)
    {
        _http = httpClient;
        _config = config;
    }

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

    public async Task SetFieldAsync(int recordId, string columnName, object value)
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

    private class RecordsResponse
    {
        [JsonPropertyName("records")]
        public List<RecordDto>? Records { get; set; }
    }

    private class RecordDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, object?>? Fields { get; set; }
    }
}
