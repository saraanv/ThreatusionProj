using System.Text.Json.Serialization;

namespace ThreatusionProj.Models;

public class NvdResponse
{
    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public List<NvdVulnerabilityItem> Vulnerabilities { get; set; } = [];
}

public class NvdVulnerabilityItem
{
    [JsonPropertyName("cve")]
    public NvdCve Cve { get; set; } = new();
}

public class NvdCve
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("descriptions")]
    public List<NvdDescription> Descriptions { get; set; } = [];
}

public class NvdDescription
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "";

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}