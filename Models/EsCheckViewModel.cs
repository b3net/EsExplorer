namespace EsCheckWeb.Models;

public class EsCheckViewModel
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public string? ClusterName { get; set; }
    public string? Version { get; set; }

    public List<string> Indices { get; set; } = new();

    public string? SelectedIndex { get; set; }
    public string? MappingJson { get; set; }
    public string? SampleDocumentJson { get; set; }

    public string? ActiveServerName { get; set; }
    public string? ActiveServerUrl { get; set; }
}