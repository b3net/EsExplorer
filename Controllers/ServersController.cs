using EsCheckWeb.Models;
using EsCheckWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace EsCheckWeb.Controllers;

public class ServersController : Controller
{
    private readonly ElasticServerStore _serverStore;

    public ServersController(ElasticServerStore serverStore)
    {
        _serverStore = serverStore;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new ServerListViewModel
        {
            Servers = await _serverStore.GetServersAsync(),
            ActiveServerId = _serverStore.GetActiveServerId()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(ServerListViewModel model)
    {
        try
        {
            await _serverStore.AddServerAsync(model.NewServer);
        }
        catch (Exception ex)
        {
            model.Servers = await _serverStore.GetServersAsync();
            model.ActiveServerId = _serverStore.GetActiveServerId();
            model.Error = ex.Message;

            return View("Index", model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Select(string serverId)
    {
        _serverStore.SetActiveServerId(serverId);

        return RedirectToAction("Index", "EsCheck");
    }
}