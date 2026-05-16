using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EsCheckWeb.Services;

public class ElasticRawService
{
    private readonly HttpClient _httpClient;
    private readonly ElasticServerStore _serverStore;

    private readonly string[] allowedMethods =
    [
        "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD"
    ];

    private readonly string[] methodsWithBody =
    [
        "POST", "PUT", "PATCH"
    ];

    // All endpoints are now allowed. Safety is handled by frontend confirmation.
    private readonly string[] blockedEndpoints = [];

    public ElasticRawService(
        HttpClient httpClient,
        ElasticServerStore serverStore)
    {
        _httpClient = httpClient;
        _serverStore = serverStore;
    }

    public async Task<ElasticResponse> ExecuteAsync(
        string? indexName,
        string endpoint,
        string method,
        string? bodyJson)
    {
        var server = await _serverStore.GetActiveServerAsync();

        var cleanEndpoint = endpoint.Trim();
        if (!cleanEndpoint.StartsWith("/"))
            cleanEndpoint = "/" + cleanEndpoint;

        string url;
        if (!string.IsNullOrWhiteSpace(indexName))
        {
            url = $"{server.Url.TrimEnd('/')}/{Uri.EscapeDataString(indexName)}{cleanEndpoint}";
        }
        else
        {
            url = $"{server.Url.TrimEnd('/')}{cleanEndpoint}";
        }

        var methodUpper = method.ToUpperInvariant();
        if (!allowedMethods.Contains(methodUpper))
        {
            throw new InvalidOperationException(
                $"Metoda {methodUpper} jest zablokowana. Dozwolone są tylko GET i POST.");
        }

        if (blockedEndpoints.Any(x =>
                cleanEndpoint.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Endpoint {cleanEndpoint} jest zablokowany w tej aplikacji.");
        }

        var httpMethod = new HttpMethod(methodUpper);
        using var request = new HttpRequestMessage(httpMethod, url);
        if (methodsWithBody.Contains(method.ToUpperInvariant())
            && !string.IsNullOrWhiteSpace(bodyJson))
        {
            // Skip standard JSON validation for bulk
            if (!cleanEndpoint.Contains("_bulk", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    JsonDocument.Parse(bodyJson);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Niepoprawny JSON: {ex.Message}");
                }
            }

            request.Content = new StringContent(
                bodyJson,
                Encoding.UTF8,
                cleanEndpoint.Contains("_bulk", StringComparison.OrdinalIgnoreCase) 
                    ? "application/x-ndjson" 
                    : "application/json");
        }

        if (!string.IsNullOrWhiteSpace(server.ApiKey))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("ApiKey", server.ApiKey);
        }
        else if (!string.IsNullOrWhiteSpace(server.Username)
              && !string.IsNullOrWhiteSpace(server.Password))
        {
            var token = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{server.Username}:{server.Password}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", token);
        }

        using var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        return new ElasticResponse
        {
            StatusCode = (int)response.StatusCode,
            StatusText = response.ReasonPhrase ?? "OK",
            Url = url,
            RawBody = FormatResponse(responseBody)
        };
    }

    public class ElasticResponse
    {
        public int StatusCode { get; set; }
        public string StatusText { get; set; } = "";
        public string Url { get; set; } = "";
        public string RawBody { get; set; } = "";
    }

    private string FormatResponse(string? responseBody) 
    {
        string formattedResponse = responseBody ?? "";

        try
        {
            using var jsonDoc = JsonDocument.Parse(responseBody ?? "");

            var options = new JsonSerializerOptions { WriteIndented = true };
            formattedResponse = JsonSerializer.Serialize(jsonDoc, options);
        }
        catch (JsonException)
        {
        }

        return formattedResponse;
    }
}