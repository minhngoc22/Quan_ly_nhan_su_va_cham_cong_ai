using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;

namespace QLNS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public UsersController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        /* ================= DANH SÁCH USER ================= */
        public async Task<IActionResult> Index(
     string? keyword,
     string? role,
     bool? isActive)
        {
            var query = _context.Users
                .Include(u => u.Employee)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            /* ================= SEARCH ================= */
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u =>
                    u.Username.Contains(keyword) ||
                    u.Employee.FullName.Contains(keyword));
            }

            /* ================= FILTER ROLE ================= */
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u =>
                    u.UserRoles.Any(r => r.Role.RoleName == role));
            }

            /* ================= FILTER STATUS ================= */
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var users = await query
                .OrderBy(u => u.Id)
                .ToListAsync();

            return View(users);
        }

        public class ResetFaceVM
        {
            public int Id { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ResetFace([FromBody] ResetFaceVM model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null)
                return Json(new { success = false });

            var faces = await _context.FaceEmbeddings
                .Where(f => f.EmployeeId == user.EmployeeId)
                .ToListAsync();

            if (faces.Any())
                _context.FaceEmbeddings.RemoveRange(faces);

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        /// ================= XÓA USER =================
        /// 
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return Json(new { success = false });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        //================== UPDATE ROLE===============
        public class UpdateUserVM
        {
            public int UserId { get; set; }
            public string? RoleName { get; set; }
            public bool? IsActive { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserVM model)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
                return Json(new { success = false });

            /* ===== UPDATE STATUS ===== */
            if (model.IsActive.HasValue)
                user.IsActive = model.IsActive.Value;

            /* ===== UPDATE ROLE ===== */
            if (!string.IsNullOrEmpty(model.RoleName))
            {
                _context.UserRoles.RemoveRange(user.UserRoles);

                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == model.RoleName);

                if (role != null)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}