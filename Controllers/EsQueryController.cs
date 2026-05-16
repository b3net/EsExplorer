using EsCheckWeb.Models;
using EsCheckWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace EsCheckWeb.Controllers;

public class EsQueryController : Controller
{
    private readonly ElasticService _elasticService;
    private readonly ElasticRawService _elasticRawService;
    private readonly ElasticServerStore _serverStore;

    public EsQueryController(
        ElasticService elasticService,
        ElasticRawService elasticRawService,
        ElasticServerStore serverStore)
    {
        _elasticService = elasticService;
        _elasticRawService = elasticRawService;
        _serverStore = serverStore;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? indexName)
    {
        var model = new EsQueryViewModel();

        try
        {
            model.Indices = await _elasticService.GetIndicesAsync();
            model.Servers = await _serverStore.GetServersAsync();
            model.SelectedIndex = indexName;
        }
        catch (Exception ex)
        {
            model.Error = ex.Message;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(EsQueryViewModel model)
    {
        try
        {
            var response = await _elasticRawService.ExecuteAsync(
                model.SelectedIndex,
                model.Endpoint,
                model.HttpMethod,
                model.BodyJson
            );

            model.ResultJson = response.RawBody;
            model.StatusCode = response.StatusCode;
            model.StatusText = response.StatusText;
            model.ResponseUrl = response.Url;
            model.Success = true;
        }
        catch (Exception ex)
        {
            model.Success = false;
            model.Error = ex.Message;
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { 
                success = model.Success, 
                result = model.ResultJson, 
                statusCode = model.StatusCode,
                statusText = model.StatusText,
                url = model.ResponseUrl,
                error = model.Error 
            });
        }

        model.Indices = await _elasticService.GetIndicesAsync();
        model.Servers = await _serverStore.GetServersAsync();
        return View(model);
    }

    [HttpPost]
    public IActionResult SwitchServer(string serverId)
    {
        _serverStore.SetActiveServerId(serverId);
        return RedirectToAction("Index");
    }
}