using Microsoft.AspNetCore.Mvc;
using Mvc.Models;
using Mvc.Services;

namespace Mvc.Controllers;

public class HomeController(ServiceBusDemoService serviceBusDemoService) : Controller
{
    private readonly ServiceBusDemoService _serviceBusDemoService = serviceBusDemoService;

    public async Task<IActionResult> Index()
    {
        var peeked = await _serviceBusDemoService.PeekAsync();

        var model = new IndexViewModel
        {
            Peeked = peeked,
            Received = null
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send([FromForm] string text)
    {
        await _serviceBusDemoService.SendAsync(text);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        var received = await _serviceBusDemoService.ReceiveAsync();
        var peeked = await _serviceBusDemoService.PeekAsync();

        var model = new IndexViewModel
        {
            Peeked = peeked,
            Received = received
        };

        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Complete([FromForm] string lockToken)
    {
        var msg = _serviceBusDemoService.GetCurrentMessage();

        if (msg != null && msg.LockToken == lockToken)
            await _serviceBusDemoService.CompleteAsync(msg);

        var peeked = await _serviceBusDemoService.PeekAsync();

        var model = new IndexViewModel
        {
            Peeked = peeked,
            Received = null
        };

        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Abandon([FromForm] string lockToken)
    {
        var msg = _serviceBusDemoService.GetCurrentMessage();

        if (msg != null && msg.LockToken == lockToken)
            await _serviceBusDemoService.AbandonAsync(msg);

        var peeked = await _serviceBusDemoService.PeekAsync();

        var model = new IndexViewModel
        {
            Peeked = peeked,
            Received = null
        };

        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> DeadLetter([FromForm] string lockToken)
    {
        var msg = _serviceBusDemoService.GetCurrentMessage();

        if (msg != null && msg.LockToken == lockToken)
            await _serviceBusDemoService.DeadLetterAsync(msg);

        var peeked = await _serviceBusDemoService.PeekAsync();

        var model = new IndexViewModel
        {
            Peeked = peeked,
            Received = null
        };

        return View("Index", model);
    }
}
