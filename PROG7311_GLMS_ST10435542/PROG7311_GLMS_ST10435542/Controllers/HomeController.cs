using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_ST10435542.Models;
using Microsoft.AspNetCore.Authorization;

namespace PROG7311_GLMS_ST10435542.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
