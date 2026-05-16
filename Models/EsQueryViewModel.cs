namespace EsCheckWeb.Models;

public class EsQueryViewModel
{
    public List<string> Indices { get; set; } = new();
    public List<ElasticServerConfig> Servers { get; set; } = new();

    public string? SelectedIndex { get; set; }

    public string HttpMethod { get; set; } = "POST";

    public string Endpoint { get; set; } = "/_search";

    public string BodyJson { get; set; } = """
    {
      "size": 10,
      "query": {
        "match_all": {}
      }
    }
    """;

    public string? ResultJson { get; set; }
    public int? StatusCode { get; set; }
    public string? StatusText { get; set; }
    public string? ResponseUrl { get; set; }
    public string? Error { get; set; }
    public bool Success { get; set; }
}