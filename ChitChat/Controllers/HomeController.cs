using ChitChat.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChitChat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            var customError = new CustomError();
            customError.code = code;
            //Well handle an 404 error and the rest.
            if (code == 404)
            {
                customError.message = "The page you are looking for might have been removed / had its name changed or is remporarily unavailable.";
            }
            else
            {
                customError.message = "Sorry, something went wrong";
            }
            //We could make views per satus code.
            return View("~/Views/Shared/CustomError.cshtml", customError);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
