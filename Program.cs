using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Text.Json;
using ThreatusionProj.Models;
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
        // 1. دریافت JSON خام از NVD
        var json = await nvdClient.GetRecentCvesAsync();

        // 2. تبدیل JSON به کلاس‌های C#
        var data = JsonSerializer.Deserialize<NvdResponse>(json);

        if (data is null)
        {
            return Results.Problem(
                title: "Invalid NVD response",
                statusCode: StatusCodes.Status502BadGateway);
        }

        // 3. انتخاب شناسه و توضیحات انگلیسی هر CVE
        var cves = data.Vulnerabilities.Select(item => new
        {
            Id = item.Cve.Id,

            Description = item.Cve.Descriptions
                .FirstOrDefault(description => description.Lang == "en")
                ?.Value ?? "No English description available"
        });

        // 4. برگرداندن خروجی مرتب‌شده
        return Results.Json(new
        {
            data.TotalResults,
            data.ResultsPerPage,
            Cves = cves
        });
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