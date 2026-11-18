using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ThreatusionProj.Services;

var builder = WebApplication.CreateBuilder(args);

// Register NvdClient and configure its HttpClient.
builder.Services.AddHttpClient<NvdClient>(client =>
{
    client.BaseAddress = new Uri(
        "https://services.nvd.nist.gov/rest/json/");
});

var app = builder.Build();

// Temporary endpoint to verify configuration.
app.MapGet("/api/nvd/config-test", (
    IConfiguration configuration,
    IHostEnvironment environment) =>
{
    return Results.Json(new
    {
        Environment = environment.EnvironmentName,
        ApiKeyLoaded = !string.IsNullOrWhiteSpace(
            configuration["Nvd:ApiKey"])
    });
});

// Endpoint to retrieve CVE data from NVD.
app.MapGet("/api/nvd/test", async (NvdClient nvdClient) =>
{
    try
    {
        var result = await nvdClient.GetRecentCvesAsync();

        return Results.Content(result, "application/json");
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            title: "Failed to retrieve data from NVD",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "NVD configuration error",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.Run();