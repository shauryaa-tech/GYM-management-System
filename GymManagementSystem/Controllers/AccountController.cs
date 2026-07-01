using GymManagement.Data.Repositories;
using GymManagement.Helpers;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AccountRepository _accountRepo;

        public AccountController(AccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = _accountRepo.Login(
                model.UserName,
                model.Password);

            if (!result.Success)
            {
                ViewBag.Error = "Invalid Login";
                return View(model);
            }

            HttpContext.Session.SetString("FullName", result.FullName);

            HttpContext.Session.SetString("UserName", model.UserName);

            HttpContext.Session.SetInt32("RoleId", result.RoleId);

            return RedirectToAction("Index", "Dashboard");


        }


        public IActionResult Logout()
        {
            string? userName =
                HttpContext.Session.GetString(
                    "UserName");

            if (!string.IsNullOrEmpty(userName))
            {
                _accountRepo.LogoutUser(
                    userName);
            }

            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Account");
        }
    }
}