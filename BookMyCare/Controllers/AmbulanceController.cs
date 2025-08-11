using Microsoft.AspNetCore.Mvc;

namespace BookMyCare.Controllers
{
    public class AmbulanceController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
    }
}
