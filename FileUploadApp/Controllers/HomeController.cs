using Microsoft.AspNetCore.Mvc;

namespace FileUploadApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult FileUpload()
        {
            return View();
        }
    }
}
