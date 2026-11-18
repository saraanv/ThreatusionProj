namespace ThreatusionProj.Services;

public class NvdClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public NvdClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetRecentCvesAsync()
    {
        var apiKey = _configuration["Nvd:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "NVD API Key is not configured.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "cves/2.0?resultsPerPage=5");

        request.Headers.Add("apiKey", apiKey);

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}