namespace EsCheckWeb.Models;

public class ServerListViewModel
{
    public List<ElasticServerConfig> Servers { get; set; } = new();

    public string? ActiveServerId { get; set; }

    public ElasticServerConfig NewServer { get; set; } = new();

    public string? Error { get; set; }
}