using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Settings", "Profile");
        }
    }
}
