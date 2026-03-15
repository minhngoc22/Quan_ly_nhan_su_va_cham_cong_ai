using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Services;
using System.Security.Claims;

namespace QLNS.Controllers
{
    public class AuthController : Controller
    {
        private readonly FaceIdHrmsContext  _db;
        private readonly LogService _logService;
        public AuthController( FaceIdHrmsContext db,LogService logService)
        {
            _db = db;
            _logService = logService;
        }

        // =========================
        // GET: LOGIN
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Success = TempData["Success"];
            ViewBag.RedirectUrl = TempData["RedirectUrl"];
            ViewBag.SelectedRole = TempData["SelectedRole"];
            ViewBag.StayOnLogin = TempData["StayOnLogin"];

            // ⭐ CHỈ redirect nếu KHÔNG phải vừa login xong
            if (ViewBag.Success == null &&
                User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Employee"))
                    return RedirectToAction("Index", "Staff");

                if (User.IsInRole("HR"))
                    return RedirectToAction("Index", "Home");

                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Department");
            }

            return View();
        }
        // =========================
        // POST: LOGIN
        // =========================
        // =========================
        // POST: LOGIN
        // =========================

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            var user = _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            bool passwordValid = false;

            if (user.PasswordHash.StartsWith("$2"))
                passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            else
            {
                passwordValid = password == user.PasswordHash;

                if (passwordValid)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    _db.SaveChanges();
                }
            }

            if (!passwordValid)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            if (string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Vui lòng chọn phân quyền";
                return View();
            }

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == role))
            {
                ViewBag.Error = "Bạn không có quyền này";
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("SelectedRole", role)
    };

            foreach (var ur in user.UserRoles)
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.RoleName));

            var identity = new ClaimsIdentity(claims, "FaceIDAuth");

            await HttpContext.SignInAsync(
                "FaceIDAuth",
                new ClaimsPrincipal(identity));

            await _logService.LogAsync(
      user.Id,
      "ĐĂNG NHẬP",
      $"Vai trò: {role}"
  );

            // gửi message qua request sau
            TempData["Success"] = "Đăng nhập thành công!";
            TempData["RedirectUrl"] = role switch
            {
                "Employee" => "/Staff",
                "HR" => "/Home",
                "Admin" => "/Admins",
                _ => "/Home"
            };

            TempData["SelectedRole"] = role;
            TempData["StayOnLogin"] = true;

            return RedirectToAction("Login");
        }


            // =========================
            // LOGOUT
            // =========================
            [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();

            if (userId != null)
            {
                await _logService.LogAsync(
                    userId.Value,
                    "ĐĂNG XUẤT",
                    "Người dùng đăng xuất khỏi hệ thống"
                );
            }

            await HttpContext.SignOutAsync("FaceIDAuth");

            return RedirectToAction("Login", "Auth");
        }

        // =========================
        // ACCESS DENIED
        // =========================
        public IActionResult AccessDenied()
        {
            return View(); // Nên tạo View đẹp thay vì Content
        }


        private int? GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id == null ? null : int.Parse(id);
        }
    }
}

