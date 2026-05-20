using Microsoft.AspNetCore.Mvc;
using Mvc.Services;

namespace Mvc.Controllers;

public class AzureFunctionHttpTriggerController(SignalRAzureServiceBusQueueService serviceBusDemoService) : Controller
{
    private readonly SignalRAzureServiceBusQueueService _serviceBusDemoService = serviceBusDemoService;

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send([FromForm] string text)
    {
        await _serviceBusDemoService.SendAsync(text);
        return RedirectToAction("Index");
    }   
}