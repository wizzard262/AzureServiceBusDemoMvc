using Microsoft.AspNetCore.Mvc;
using Mvc.Models;
using Mvc.Services;

namespace Mvc.Controllers;

public class TopicController(AzureServiceBusTopicService serviceBusDemoService) : Controller
{
    private readonly AzureServiceBusTopicService _serviceBusDemoService = serviceBusDemoService;

    public async Task<IActionResult> Index()
    {
        var model = new TopicViewModel
        {
        };
        return View(model);
    }
}
