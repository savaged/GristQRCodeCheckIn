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
