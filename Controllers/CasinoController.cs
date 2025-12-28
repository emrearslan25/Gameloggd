using Microsoft.AspNetCore.Mvc;

namespace GameLoggd.Controllers
{
    public class CasinoController : Controller
    {
        [HttpGet("/casino")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
