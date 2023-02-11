using Microsoft.AspNetCore.Mvc;
using NuJournalPro.Models;
using NuJournalPro.Models.ViewModels;
using NuJournalPro.Services.Interfaces;
using System.Diagnostics;

namespace NuJournalPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServerService _serverService;

        public HomeController(ILogger<HomeController> logger,
                              IServerService serverService)
        {
            _logger = logger;
            _serverService = serverService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/Home/ErrorHandler/{code:int}")]
        public IActionResult ErrorHandler(int code)
        {
            var customError = new CustomError();
            customError.code = code;
            customError.message = _serverService.GetErrorMessageString(code);

            return View("~/Views/Shared/ErrorPage.cshtml", customError);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}