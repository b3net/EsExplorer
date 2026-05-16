using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using EsCheckWeb.Models;

namespace EsCheckWeb.Services;

public class ElasticService
{
    private readonly ElasticServerStore _serverStore;

    public ElasticService(ElasticServerStore serverStore)
    {
        _serverStore = serverStore;
    }

    private async Task<ElasticsearchClient> CreateClientAsync()
    {
        var server = await _serverStore.GetActiveServerAsync();

        var settings = new ElasticsearchClientSettings(new Uri(server.Url))
            .PrettyJson()
            .EnableDebugMode();

        if (!string.IsNullOrWhiteSpace(server.ApiKey))
        {
            settings = settings.Authentication(new ApiKey(server.ApiKey));
        }
        else if (!string.IsNullOrWhiteSpace(server.Username) &&
                 !string.IsNullOrWhiteSpace(server.Password))
        {
            settings = settings.Authentication(
                new BasicAuthentication(server.Username, server.Password));
        }

        return new ElasticsearchClient(settings);
    }

    public async Task<(string? ClusterName, string? Version)> GetInfoAsync()
    {
        var client = await CreateClientAsync();

        var response = await client.InfoAsync();

        if (!response.IsValidResponse)
            throw new Exception(response.DebugInformation);

        return (response.ClusterName, response.Version?.Number);
    }

    public async Task<List<string>> GetIndicesAsync()
    {
        var client = await CreateClientAsync();

        var response = await client.Indices.GetAsync(Indices.All);

        if (!response.IsValidResponse)
            throw new Exception(response.DebugInformation);

        return response.Indices.Keys
            .Select(x => x.ToString())
            .OrderBy(x => x)
            .ToList();
    }

    public async Task<string> GetMappingAsync(string indexName)
    {
        var client = await CreateClientAsync();

        var response = await client.Indices.GetMappingAsync(indexName);

        if (!response.IsValidResponse)
            throw new Exception(response.DebugInformation);

        return response.DebugInformation;
    }

    public async Task<string> GetSampleDocumentAsync(string indexName)
    {
        var client = await CreateClientAsync();

        var response = await client.SearchAsync<object>(s => s
            .Indices(indexName)
            .Size(1)
            .Query(q => q.MatchAll(new Elastic.Clients.Elasticsearch.QueryDsl.MatchAllQuery()))
        );

        if (!response.IsValidResponse)
            throw new Exception(response.DebugInformation);

        return JsonSerializer.Serialize(response.Documents, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}