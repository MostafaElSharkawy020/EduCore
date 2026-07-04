using EduCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EduCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // The app opens on the login page. Signed-in users go to their home instead.
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Teacher"))
                    return RedirectToAction("Index", "Dashboard");
                if (User.IsInRole("Student"))
                    return RedirectToAction("Index", "Learn");
            }

            return RedirectToAction("Login", "Account");
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
}
