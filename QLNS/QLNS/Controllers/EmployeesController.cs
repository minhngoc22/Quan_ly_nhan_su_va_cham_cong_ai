using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using System.Security.Claims;
using QLNS.Models.DTO;
using QLNS.Models.ViewModels;
using QLNS.Services;
using System.Reflection.Emit;
using DocumentFormat.OpenXml.Math;
using BCrypt.Net;


namespace QLNS.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeesController : Controller
    {
        private readonly FaceIdHrmsContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly LogService _logService; // ⭐ THÊM

        public EmployeesController(FaceIdHrmsContext db, IWebHostEnvironment env, LogService logService)
        {
            _db = db;
            _env = env;
            _logService = logService; // ⭐ THÊM
        }

        // ================================
        // 1. DANH SÁCH NHÂN VIÊN
        // ================================
        public IActionResult Index(string search, int? departmentId, string status, int? hireMonth)
        {
            // ===== LẤY USER ĐANG ĐĂNG NHẬP =====
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = _db.Users
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Department)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Position)
               .Include(u => u.UserRoles)
    .ThenInclude(ur => ur.Role)
                .First(u => u.Id == userId);

            // ===== FILL DROPDOWN =====
            ViewBag.Departments = _db.Departments.ToList();

            // ===== QUERY NHÂN VIÊN =====
            var employeesQuery = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                employeesQuery = employeesQuery.Where(e =>
                    e.FullName.Contains(search) ||
                    e.EmployeeCode.Contains(search) ||
                    e.Email.Contains(search));
            }

            // 🏢 PHÒNG BAN
            if (departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
            }

            // 📌 TRẠNG THÁI
            if (!string.IsNullOrWhiteSpace(status))
            {
                employeesQuery = employeesQuery.Where(e => e.Status == status);
            }

            //Ngày thuê
            // 📅 THÁNG VÀO LÀM (CHỈ 1 CÁI)
            if (hireMonth.HasValue)
            {
                employeesQuery = employeesQuery.Where(e =>
                    e.HireDate.HasValue &&
                    e.HireDate.Value.Month == hireMonth.Value);
            }


            // ===== DATA =====
            ViewBag.Employees = employeesQuery.ToList();

            // ===== GIỮ TRẠNG THÁI FILTER =====
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentDeptId = departmentId;
            ViewBag.CurrentStatus = status;

            ViewBag.CurrentHireMonth = hireMonth;


            // ===== THỐNG KÊ =====
            ViewBag.TotalEmployees = _db.Employees.Count();
            var today =     DateTime.Today;
            ViewBag.TodayCheckin = _db.Attendances.Count(x => x.WorkDate == today);

            return View(user);
        }

        // ================================
        // 2. CHI TIẾT NHÂN VIÊN
        // ================================
        public IActionResult Details(int id)
        {
            var employee = _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                 .Include(e => e.ExperienceLevel) // ⭐ THÊM
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(); // Hoặc redirect về Index + thông báo
            }

            return View(employee);
        }


        // ================================
        // 3. FORM THÊM NHÂN VIÊN
        // ================================
        public IActionResult Create()
        {
            ViewBag.Departments = _db.Departments.ToList();
            ViewBag.Positions = _db.Positions.ToList();
            ViewBag.ExperienceLevels = _db.ExperienceLevels.ToList();

            var vm = new EmployeeCreateVM
            {
                HireDate = DateTime.Today,
                Status = "Đang làm"
            };

            return View(vm);
        }


        // ================================
        // 4. XỬ LÝ THÊM
        // ================================

        private void LoadCombos()
        {
            ViewBag.Departments = _db.Departments.ToList();
            ViewBag.Positions = _db.Positions.ToList();
            ViewBag.ExperienceLevels = _db.ExperienceLevels.ToList();
        }


        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateVM model, IFormFile avatarFile)
        {
            if (!ModelState.IsValid)
            {
                LoadCombos();
                return View(model);
            }

            // 1️⃣ LẤY CODE PHÒNG + CHỨC VỤ
            var deptCode = _db.Departments
                .Where(d => d.Id == model.DepartmentId)
                .Select(d => d.DepartmentCode)
                .FirstOrDefault();

            var posCode = _db.Positions
                .Where(p => p.Id == model.PositionId)
                .Select(p => p.PositionCode)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(deptCode) || string.IsNullOrEmpty(posCode))
            {
                ModelState.AddModelError("", "Phòng ban hoặc chức vụ không hợp lệ");
                LoadCombos();
                return View(model);
            }

            // 2️⃣ SINH MÃ NHÂN VIÊN (HR-NV-001)
            string prefix = $"{deptCode}-{posCode}-";

            int nextNumber = _db.Employees
                .Where(e => e.EmployeeCode.StartsWith(prefix))
                .Select(e => e.EmployeeCode.Substring(prefix.Length))
                .AsEnumerable() // ⭐ QUAN TRỌNG
                .Select(s => int.TryParse(s, out int n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            string employeeCode = $"{prefix}{nextNumber:000}";


            // 3️⃣ TẠO NHÂN VIÊN
            var emp = new Employee
            {
                EmployeeCode = employeeCode,
                FullName = model.FullName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                DepartmentId = model.DepartmentId,
                PositionId = model.PositionId,
                ExperienceLevelId = model.ExperienceLevelId,
                HireDate = model.HireDate,
                Status = model.Status,
                Avatar = "/images/default.jpg"
            };

            // 4️⃣ AVATAR
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "images", "avatars");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                avatarFile.CopyTo(stream);

                emp.Avatar = "/images/avatars/" + fileName;
            }

            _db.Employees.Add(emp);
            await _db.SaveChangesAsync();

            ////ghi log
            //await _logService.LogAsync(
            //    int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
            //    "Thêm Nhân Viên Mới",
            //    $"Thêm nhân viên: {model.FullName} - {employeeCode}"
            //);

            // 2️⃣ Tạo User login cho nhân viên
            var defaultPassword = "123456";

            var user = new User
            {
                EmployeeId = emp.Id,
                Username = emp.EmployeeCode,
                Email = emp.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                IsFirstLogin = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // ================================
            // GÁN ROLE EMPLOYEE
            // ================================
            var employeeRole = _db.Roles.FirstOrDefault(r => r.RoleName == "Employee");

            if (employeeRole == null)
            {
                throw new Exception("❌ Role Employee chưa tồn tại trong DB");
            }

            // thêm user trước
            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // cần để có UserId

            // tạo bảng nối UserRole
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = employeeRole.Id
            };

            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();

            // ================================
            _db.Users.Add(user);
            await _db.SaveChangesAsync();


            await _logService.LogAsync(
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                "THÊM NHÂN VIÊN",
                $"EmployeeCode={employeeCode}; " +
                $"FullName={model.FullName}; " +
                $"Username={user.Username}; " +
                $"DefaultPassword=Yes"
            );


            TempData["CreateUserSuccess"] = true;
            TempData["Username"] = user.Username;
            TempData["DefaultPassword"] = defaultPassword;
            TempData["RedirectUrl"] = Url.Action("Details", new { id = emp.Id });

            LoadCombos(); // ⚠️ BẮT BUỘC vì return View
            return View(model);



        }


        // ================================
        // 5. FORM SỬA
        // ================================
        // ================================
        // 5. FORM SỬA (EMPLOYEE PROFILE)
        // ================================
        // ================================
        // 5. FORM SỬA
        // ================================
        // ================================
        // 5. FORM SỬA (EMPLOYEE PROFILE)
        // ================================
        public IActionResult Edit(int id)
        {
            var emp = _db.Employees.FirstOrDefault(e => e.Id == id);
            if (emp == null) return NotFound();

            ViewBag.Departments = _db.Departments.ToList();
            ViewBag.Positions = _db.Positions.ToList();

            // ⭐ LOAD KINH NGHIỆM
            ViewBag.ExperienceLevels = _db.ExperienceLevels.ToList();

            var vm = new EmployeeProfileVM
            {
                Id = emp.Id,
                EmployeeCode = emp.EmployeeCode,
                FullName = emp.FullName,
                Gender = emp.Gender,
                Email = emp.Email,
                Phone = emp.Phone,
                Address = emp.Address,
                Status = emp.Status,
                DepartmentId = emp.DepartmentId ?? 0,
                PositionId = emp.PositionId ?? 0,
                ExperienceLevelId = emp.ExperienceLevelId, // ⭐
                Avatar = emp.Avatar,
                DateOfBirth = emp.DateOfBirth




            };

            return View(vm);
        }


        // ================================
        // 6. XỬ LÝ SỬA
        // ================================
        // ================================

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeProfileVM model, IFormFile avatarFile)

        {
            // ❌ Validate fail → load lại dropdown
            foreach (var entry in ModelState)
            {
                if (entry.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"❌ FIELD: {entry.Key}");
                    foreach (var err in entry.Value.Errors)
                    {
                        Console.WriteLine("   ➜ " + err.ErrorMessage);
                    }
                }
            }


            // 🔍 LẤY NHÂN VIÊN TỪ DB
            var empDb = _db.Employees.FirstOrDefault(e => e.Id == model.Id);
            if (empDb == null)
                return NotFound();

            // =========================
            // UPDATE THÔNG TIN CƠ BẢN
            // =========================
            // UPDATE THÔNG TIN CƠ BẢN
            empDb.FullName = model.FullName;
            empDb.Gender = model.Gender;              // ⭐ THÊM
            empDb.DateOfBirth = model.DateOfBirth;

            empDb.Email = model.Email;
            empDb.Phone = model.Phone;
            empDb.Address = model.Address;
            empDb.Status = model.Status;


            // 🔒 KHÔNG SỬA MÃ NV
            // empDb.EmployeeCode = model.EmployeeCode; ❌ KHÔNG ĐỤNG

            // 🔒 PHÒNG BAN KHÓA
            empDb.DepartmentId = model.DepartmentId;

            // ✅ CÓ THỂ CHO SỬA / HOẶC KHÓA TUỲ Ý
            empDb.PositionId = model.PositionId;

            // ⭐ KINH NGHIỆM
            empDb.ExperienceLevelId = model.ExperienceLevelId;

            // =========================
            // UPDATE AVATAR
            // =========================
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "images", "avatars");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    avatarFile.CopyTo(stream);
                }

                empDb.Avatar = "/images/avatars/" + fileName;
            }

            // =========================
            // SAVE
            // =========================
           
            _db.SaveChanges(); // ✅ LƯU TRƯỚC

            await _logService.LogAsync(
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                "CẬP NHẬT HỒ SƠ NHÂN VIÊN",
                $"Cập nhật nhân viên: {empDb.FullName}- {empDb.EmployeeCode}"
            );


            // 👉 QUAY VỀ DETAILS
            return RedirectToAction("Details", new { id = empDb.Id });
        }




        // ================================
        // 7. XÓA
        // ================================
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = _db.Users
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Department)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Position)
                .Include(u => u.UserRoles)
    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return Unauthorized();

            // ===== ADMIN => XÓA =====
            if (user.UserRoles.Any(ur => ur.Role.RoleName == "Admin"))
            {
                return await DeleteConfirmed(id);
            }

            // ===== HR => CHECK MÃ =====
            if (user.UserRoles.Any(ur => ur.Role.RoleName == "HR"))
            {
                var empCode = user.Employee?.EmployeeCode;

                if (empCode != null &&
                    (empCode.Contains("HR-TP") || empCode.Contains("HR-PP")))
                {
                    return await DeleteConfirmed(id);
                }
            }

            TempData["Error"] = "❌ Bạn không có quyền xóa nhân viên";
            return RedirectToAction("Index");
        }

        private async Task<IActionResult> DeleteConfirmed(int id)
        {
            var emp = _db.Employees.Find(id);
            if (emp == null)
                return NotFound();

            // ⭐ LƯU CODE TRƯỚC KHI XÓA
            var empCode = emp.EmployeeCode;

            _db.Employees.Remove(emp);
            await _db.SaveChangesAsync();

            try
            {
                await _logService.LogAsync(
                    int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    "XÓA NHÂN VIÊN",
                    $"Xóa nhân viên {empCode}"
                );
            }
            catch { }


            TempData["Success"] = $"🗑️ Đã xóa nhân viên {empCode}";
            return RedirectToAction("Index");
        }



        // ================================
        // 8. ĐĂNG KÝ KHUÔN MẶT


        [HttpPost]
        public async Task<IActionResult> RegisterFace([FromBody] RegisterFaceRequest req)
        {
            using var client = new HttpClient();

            var response = await client.PostAsync(
              $"http://127.0.0.1:8000/register_face/{req.EmployeeCode}",

                null
            );

            if (!response.IsSuccessStatusCode)
                return Json(new
                {
                    success = false,
                    message = "Không kết nối được FaceID service"
                });

            try
            {
                await _logService.LogAsync(
                    int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    "ĐĂNG KÝ KHUÔN MẶT NHÂN VIÊN",
                    $"Đăng ký FaceID cho nhân viên {req.EmployeeCode}"
                );
            }
            catch { }


            return Json(new
            {
                success = true,
                message = "Đã mở camera để đăng ký khuôn mặt"
            });
        }

        // ================================
        // 9. ĐĂNG KÝ DÁNG NGƯỜI

        [HttpPost]
        public async Task<IActionResult> RegisterBody([FromBody] RegisterBodyRequest req)
        {
            using var client = new HttpClient();

            var response = await client.PostAsync(
                $"http://127.0.0.1:8000/register_body/{req.EmployeeCode}",
                null
            );

            if (!response.IsSuccessStatusCode)
                return Json(new
                {
                    success = false,
                    message = "Không kết nối được BodyID service"
                });

            try
            {
                await _logService.LogAsync(
                    int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    "ĐĂNG KÝ DÁNG NGƯỜI NHÂN VIÊN",
                    $"Đăng ký BodyID cho nhân viên {req.EmployeeCode}"
                );
            }
            catch { }

            return Json(new
            {
                success = true,
                message = "Đã mở camera để đăng ký dáng người"
            });
        }
    }
}
