using EsCheckWeb.Models;
using EsCheckWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace EsCheckWeb.Controllers;

public class EsCheckController : Controller
{
    private readonly ElasticService _elasticService;
    private readonly ElasticServerStore _serverStore;

    public EsCheckController(
        ElasticService elasticService,
        ElasticServerStore serverStore)
    {
        _elasticService = elasticService;
        _serverStore = serverStore;
    }

    public async Task<IActionResult> Index(string? indexName)
    {
        var model = new EsCheckViewModel();

        try
        {
            var server = await _serverStore.GetActiveServerAsync();

            model.ActiveServerName = server.Name;
            model.ActiveServerUrl = server.Url;

            var info = await _elasticService.GetInfoAsync();
            model.ClusterName = info.ClusterName;
            model.Version = info.Version;

            model.Indices = await _elasticService.GetIndicesAsync();
            model.Success = true;

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                model.SelectedIndex = indexName;
                model.MappingJson = await _elasticService.GetMappingAsync(indexName);
                model.SampleDocumentJson = await _elasticService.GetSampleDocumentAsync(indexName);
            }
        }
        catch (Exception ex)
        {
            model.Success = false;
            model.Error = ex.Message;
        }

        return View(model);
    }
}