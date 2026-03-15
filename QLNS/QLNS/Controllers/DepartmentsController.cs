using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;

namespace QLNS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public DepartmentsController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        /* ================= LIST ================= */
        public async Task<IActionResult> Index()
        {
            var data = await _context.Departments
                .Include(d => d.Employees) // ⭐ để đếm nhân viên
                .OrderBy(d => d.DepartmentCode)
                .ToListAsync();

            return View(data);
        }

        /* ======================================================
           ================= API CREATE =================
        ====================================================== */
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Department model)
        {
            if (string.IsNullOrWhiteSpace(model.DepartmentCode))
                return Json(new { success = false, message = "Thiếu mã phòng ban" });

            bool exists = await _context.Departments
                .AnyAsync(x => x.DepartmentCode == model.DepartmentCode);

            if (exists)
                return Json(new
                {
                    success = false,
                    message = "Mã phòng ban đã tồn tại"
                });

            _context.Departments.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /* ======================================================
           ================= API GET EDIT =================
        ====================================================== */
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var dept = await _context.Departments.FindAsync(id);

            if (dept == null)
                return Json(new { success = false });

            return Json(dept);
        }

        /* ======================================================
           ================= API UPDATE =================
        ====================================================== */
        [HttpPost]
        public async Task<IActionResult> EditAjax([FromBody] Department model)
        {
            var dept = await _context.Departments.FindAsync(model.Id);

            if (dept == null)
                return Json(new { success = false });

            dept.DepartmentCode = model.DepartmentCode;
            dept.DepartmentName = model.DepartmentName;
            dept.Description = model.Description;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /* ======================================================
           ================= API DELETE =================
        ====================================================== */
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept == null)
                return Json(new { success = false });

            if (dept.Employees.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xóa phòng ban đang có nhân viên"
                });
            }

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /* ================= PRINT ================= */
        public async Task<IActionResult> Print()
        {
            var data = await _context.Departments
                .Include(d => d.Employees) // ⭐ FIX thiếu include
                .OrderBy(d => d.DepartmentCode)
                .ToListAsync();

            return View(data);
        }
    }
}