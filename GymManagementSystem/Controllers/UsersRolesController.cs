using GymManagement.Data.Repositories;
using GymManagement.Models;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using BCrypt.Net;
using GymManagement.Helpers;

namespace GymManagement.Controllers
{
    public class UsersRolesController : Controller
    {
        private readonly UserRepository _userRepo;
        private readonly RoleRepository _roleRepo;
        private readonly IWebHostEnvironment _env;
        private readonly PermissionRepository _permissionRepo;
        private readonly RolePermissionRepository _rolePermissionRepo;

        public UsersRolesController(
        UserRepository userRepo,
        RoleRepository roleRepo,
        PermissionRepository permissionRepo,
        RolePermissionRepository rolePermissionRepo,
        IWebHostEnvironment env)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _env = env;
        }

        [Permission("UsersRoles", "View")]
        public IActionResult Index(string? search, string? tab)
        {
            ViewBag.ActiveTab = tab;

            var viewModel = new UsersRolesViewModel
            {
                Users = _userRepo.GetAll(search),
                Roles = _roleRepo.GetAll(),
                Permissions = new List<RolePermission>()
            };

            ViewBag.Search = search;
            ViewBag.Roles = viewModel.Roles;
            ViewBag.Modules = _permissionRepo.GetAll();

            return View(viewModel);
        }

        [HttpPost]
        [Permission("UsersRoles", "Add")]
        public IActionResult SaveUser(UserMaster model, IFormFile? photo)
        {
            try
            {
                if (photo != null && photo.Length > 0)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "Profiles");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);

                    using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                    {
                        photo.CopyTo(stream);
                    }

                    model.ProfilePhoto = "/uploads/Profiles/" + fileName;
                }

                model.IsActive = true;
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);

                _userRepo.Insert(model);

                TempData["Success"] = "User Added Successfully.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Email already exists"))
                {
                    TempData["Error"] = "This Email is already registered.";
                }
                else if (ex.Message.Contains("Username already exists"))
                {
                    TempData["Error"] = "This Username is already taken.";
                }
                else
                {
                    TempData["Error"] = ex.Message;
                }

                return RedirectToAction(nameof(Index));
            }
        }

        [Permission("UsersRoles", "Edit")]
        public IActionResult EditUser(int id)
        {
            try
            {
                ViewBag.Roles = _roleRepo.GetAll();

                var user = _userRepo.GetById(id);

                if (user == null || user.UserId == 0)
                    return NotFound();

                return PartialView("_EditUser", user);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpPost]
        [Permission("UsersRoles", "Edit")]
        public IActionResult UpdateUser(UserMaster model, IFormFile? photo)
        {
            try
            {
                var oldUser = _userRepo.GetById(model.UserId);

                if (oldUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (photo != null && photo.Length > 0)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "Profiles");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);

                    string filePath = Path.Combine(folder, fileName);

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(stream);
                    }

                    model.ProfilePhoto = "/uploads/Profiles/" + fileName;
                }
                else
                {
                    model.ProfilePhoto = oldUser.ProfilePhoto;
                }

                if (!string.IsNullOrWhiteSpace(model.PasswordHash))
                {
                    model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                }
                else
                {
                    model.PasswordHash = oldUser.PasswordHash;
                }

                _userRepo.Update(model);

                TempData["Success"] = "User Updated Successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [Permission("UsersRoles", "Delete")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _userRepo.GetById(id);

                if (user == null || user.UserId == 0)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                _userRepo.Delete(id);

                TempData["Success"] = "User Deleted Successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Permission("UsersRoles", "Edit")]
        public IActionResult SavePermission(UsersRolesViewModel vm)
        {
            try
            {
                _rolePermissionRepo.Save(vm.Role.RoleId, vm.Permissions);

                TempData["Success"] = DateTime.Now.ToString();

                return RedirectToAction(nameof(Permissions), new { id = vm.Role.RoleId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Permissions), new { id = vm.Role.RoleId });
            }
        }

        [HttpPost]
        [Permission("UsersRoles", "Add")]
        public IActionResult SaveRole(RoleMaster model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.RoleName))
                {
                    TempData["Error"] = "Role Name is required.";
                    return RedirectToAction(nameof(Index));
                }

                _roleRepo.Insert(model);

                TempData["Success"] = "Role Added Successfully.";

                return RedirectToAction("Index", new { tab = "roles" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", new { tab = "roles" });
            }
        }

        [Permission("UsersRoles", "Edit")]
        public IActionResult EditRole(long id)
        {
            try
            {
                var role = _roleRepo.GetById(id);

                if (role == null || role.RoleId == 0)
                    return NotFound();

                return PartialView("_EditRole", role);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [Permission("UsersRoles", "View")]
        public IActionResult Permissions(long id)
        {
            RolePermissionViewModel vm = new();

            vm.Role = _roleRepo.GetById(id);

            vm.Permissions = _rolePermissionRepo.GetByRole(id);

            ViewBag.Modules = _permissionRepo.GetAll();

            return View(vm);
        }

        [HttpPost]
        [Permission("UsersRoles", "Edit")]
        public IActionResult UpdateRole(RoleMaster model)
        {
            try
            {
                _roleRepo.Update(model);

                TempData["Success"] = "Role Updated Successfully.";

                return RedirectToAction("Index", new { tab = "roles" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Index", new { tab = "roles" });
            }
        }

        [Permission("UsersRoles", "Delete")]
        public IActionResult DeleteRole(long id)
        {
            try
            {
                _roleRepo.Delete(id);

                TempData["Success"] = "Role Deleted Successfully.";

                return RedirectToAction("Index", new { tab = "roles" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Index", new { tab = "roles" });
            }
        }

    }

}
