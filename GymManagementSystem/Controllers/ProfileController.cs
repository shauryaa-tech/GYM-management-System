using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AccountRepository _accountRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;
        private readonly IWebHostEnvironment _env;

        public ProfileController(
            AccountRepository accountRepo,
            RolePermissionRepository rolePermissionRepo,
            IWebHostEnvironment env)
        {
            _accountRepo = accountRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "Account");

            var profile = _accountRepo.GetProfile(userName);
            if (profile == null)
                return RedirectToAction("Login", "Account");

            return View(profile);
        }

        [HttpGet]
        public IActionResult Settings()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            return View(new ChangePasswordViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "New password and confirmation do not match.");
                return View(model);
            }

            var result = _accountRepo.ChangePassword(userName, model.CurrentPassword, model.NewPassword);
            if (!result)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateProfile([FromBody] ProfileViewModel model)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return Json(new { success = false, message = "Session expired" });

            if (string.IsNullOrWhiteSpace(model.FullName))
                return Json(new { success = false, message = "Full name is required." });

            var result = _accountRepo.UpdateProfile(userName, model.FullName, model.Email ?? "");
            if (result)
            {
                HttpContext.Session.SetString("FullName", model.FullName);
                return Json(new { success = true, message = "Profile updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update profile." });
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return Json(new { success = false, message = "Session expired" });

            if (avatar == null || avatar.Length == 0)
                return Json(new { success = false, message = "No file uploaded" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif" };
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, message = "Invalid file type. Allowed: JPG, PNG, GIF, WebP, AVIF" });

            if (avatar.Length > 5 * 1024 * 1024)
                return Json(new { success = false, message = "File size must be less than 5MB" });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "Profiles");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/Profiles/{fileName}";
            var result = _accountRepo.UpdateProfilePhoto(userName, relativePath);

            if (result)
            {
                return Json(new { success = true, message = "Avatar updated successfully", photoUrl = relativePath });
            }

            System.IO.File.Delete(filePath);
            return Json(new { success = false, message = "Failed to update avatar" });
        }

        [HttpPost]
        public IActionResult DeleteAvatar()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return Json(new { success = false, message = "Session expired" });

            // Delete the physical file if it exists and is not the default
            var currentPhoto = _accountRepo.GetProfilePhoto(userName);
            if (!string.IsNullOrEmpty(currentPhoto) && !currentPhoto.Contains("default-user"))
            {
                var filePath = Path.Combine(_env.WebRootPath, currentPhoto.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Update database to remove photo
            var result = _accountRepo.UpdateProfilePhoto(userName, null);
            if (result)
            {
                return Json(new { success = true, message = "Photo removed successfully" });
            }

            return Json(new { success = false, message = "Failed to remove photo" });
        }
    }
}