using System.Text.Json;
using EsCheckWeb.Models;

namespace EsCheckWeb.Services;

public class ElasticServerStore
{
    private readonly string _filePath;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string ActiveServerSessionKey = "ActiveElasticServerId";

    public ElasticServerStore(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "es-servers.json");
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ElasticServerConfig>> GetServersAsync()
    {
        if (!File.Exists(_filePath))
        {
            var empty = new ElasticServersFile();
            await SaveFileAsync(empty);
        }

        var json = await File.ReadAllTextAsync(_filePath);

        var file = JsonSerializer.Deserialize<ElasticServersFile>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return file?.Servers ?? new List<ElasticServerConfig>();
    }

    public async Task AddServerAsync(ElasticServerConfig server)
    {
        if (string.IsNullOrWhiteSpace(server.Name))
            throw new ArgumentException("Nazwa serwera jest wymagana.");

        if (string.IsNullOrWhiteSpace(server.Url))
            throw new ArgumentException("Adres URL serwera jest wymagany.");

        var servers = await GetServersAsync();

        server.Id = string.IsNullOrWhiteSpace(server.Id)
            ? Guid.NewGuid().ToString("N")
            : server.Id.Trim();

        if (servers.Any(x => x.Id == server.Id))
            throw new ArgumentException("Serwer z takim ID już istnieje.");

        servers.Add(server);

        await SaveFileAsync(new ElasticServersFile
        {
            Servers = servers
        });
    }

    public async Task<ElasticServerConfig> GetActiveServerAsync()
    {
        var servers = await GetServersAsync();

        if (!servers.Any())
            throw new InvalidOperationException("Brak skonfigurowanych serwerów Elasticsearch.");

        var activeId = GetActiveServerId();

        var server = servers.FirstOrDefault(x => x.Id == activeId)
            ?? servers.First();

        SetActiveServerId(server.Id);

        return server;
    }

    public string? GetActiveServerId()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(ActiveServerSessionKey);
    }

    public void SetActiveServerId(string serverId)
    {
        _httpContextAccessor.HttpContext?.Session.SetString(ActiveServerSessionKey, serverId);
    }

    public async Task<ElasticServerConfig?> GetServerByIdAsync(string id)
    {
        var servers = await GetServersAsync();
        return servers.FirstOrDefault(x => x.Id == id);
    }

    private async Task SaveFileAsync(ElasticServersFile file)
    {
        var json = JsonSerializer.Serialize(file, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_filePath, json);
    }
}