using Microsoft.AspNetCore.Mvc;
using NuJournalPro.Models;
using NuJournalPro.Models.ViewModels;
using NuJournalPro.Services;
using NuJournalPro.Services.Interfaces;
using System.Diagnostics;

namespace NuJournalPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContactEmailSender _contactEmailSender;
        private readonly IServerService _serverService;

        public HomeController(ILogger<HomeController> logger,                              
                              IContactEmailSender contactEmailSender,
                              IServerService serverService)
        {
            _logger = logger;
            _contactEmailSender = contactEmailSender;
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

        public IActionResult Contact(string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            ContactUs contactUsModel = new();
            return View(contactUsModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactUs model)
        {
            // This is where we would send the email to the site owner
            if (ModelState.IsValid)
            {
                try
                {
                    model.Message = $"{model.Message}<br /><hr /><br />{model.Name} Phone: {model.Phone}";
                    await _contactEmailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);
                    return RedirectToAction("Contact", "Home", new { swalMessage = "Success: Your message has been sent!" });
                }
                catch
                {
                    return RedirectToAction("Contact", "Home", new { swalMessage = "Error: Your message was not sent." });
                    throw;
                }
            }
            return View(model);
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