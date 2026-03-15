using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Models.ViewModels;
using QLNS.Services;
using System.Security.Claims;

namespace QLNS.Controllers
{
    [Authorize(AuthenticationSchemes = "FaceIDAuth")]


    public class AttendanceController : Controller
    {
        private readonly FaceIdHrmsContext _context;
        private readonly LogService _logService;

        private readonly HttpClient _httpClient;

        public AttendanceController(
            FaceIdHrmsContext context,
            LogService logService,
            IHttpClientFactory factory)
        {
            _context = context;
            _logService = logService;
            _httpClient = factory.CreateClient();
        }


        public IActionResult Index(
    string? keyword,
    int? departmentId,
    DateTime? date,
    string? status)
        {
            // ===== 1. Ngày làm việc =====
            var workDate = (date ?? DateTime.Today).Date;
            var nextDate = workDate.AddDays(1);

            // ===== 2. Lấy nhân viên CÓ lịch làm =====
            var employeesQuery = _context.Employees
                .Include(e => e.Department)
                .Where(e => _context.WorkSchedules.Any(ws =>
                    ws.EmployeeId == e.Id &&
                    ws.WorkDate >= workDate &&
                    ws.WorkDate < nextDate
                ))
                .AsQueryable();

            // ===== 3. Filter keyword =====
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                employeesQuery = employeesQuery.Where(e =>
                    e.FullName.Contains(keyword) ||
                    e.EmployeeCode.Contains(keyword));
            }

            // ===== 4. Filter phòng ban =====
            if (departmentId.HasValue)
            {
                employeesQuery =
                    employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
            }

            // ===== 5. Load nhân viên =====
            var employeeList = employeesQuery.ToList();

            var employeeIds = employeeList.Select(e => e.Id).ToList();

            // ===== 6. Load attendance 1 lần (ANTI N+1) =====
            var attendances = _context.Attendances
                .Where(a =>
                    employeeIds.Contains(a.EmployeeId) &&
                    a.WorkDate >= workDate &&
                    a.WorkDate < nextDate)
                .ToList();

            // ===== 7. Map ViewModel =====
            var data = employeeList.Select(e =>
            {
                var attendance = attendances
                    .Where(a => a.EmployeeId == e.Id)
                    .OrderBy(a => a.CheckTime)
                    .FirstOrDefault();

                string statusText;

                if (attendance == null)
                    statusText = "Chưa chấm công";
                else if (attendance.Status == "Trễ")
                    statusText = "Đi trễ";
                else
                    statusText = "Đúng giờ";

                return new AttendanceRowViewModel
                {
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    DepartmentName = e.Department?.DepartmentName ?? "",

                    CheckIn = attendance != null
                        ? TimeOnly.FromDateTime(attendance.CheckTime)
                        : (TimeOnly?)null,

                    Status = statusText
                };
            }).ToList();

            // ===== 8. Filter trạng thái =====
            if (!string.IsNullOrEmpty(status))
            {
                data = data.Where(x => x.Status == status).ToList();
            }

            // ===== 9. Dropdown phòng ban =====
            ViewBag.Departments = _context.Departments.ToList();

            // ===== 10. Return View =====
            return View(new AttendanceFilterViewModel
            {
                Keyword = keyword,
                DepartmentId = departmentId,
                Date = workDate,
                Status = status,
                Data = data
            });
        }




        [HttpPost]
        public async Task<IActionResult> CheckIn()
        {
            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsync(
                    "http://127.0.0.1:8000/checkin",
                    null
                );

                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    await _logService.LogAsync(
    GetCurrentUserId(),
    "CHẤM CÔNG THẤT BẠI",
    "FaceID Service lỗi"
);

                    return Json(new
                    {
                        success = false,
                        message = "FaceID Service lỗi"
                    });
                }

                // ✅ LOG SUCCESS
                await _logService.LogAsync(
    GetCurrentUserId(),
    "CHẤM CÔNG FACEID",
    "Nhận diện khuôn mặt thành công"
);

                return Content(body, "application/json");
            }
            catch
            {
                await _logService.LogAsync(
    GetCurrentUserId(),
    "LỖI CHẤM CÔNG",
    "Không kết nối được FaceID Service"
);

                return Json(new
                {
                    success = false,
                    message = "Không kết nối được FaceID Service"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CheckBody()
        {
            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsync(
      "http://127.0.0.1:8000/check-body",
      null
  );

                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Body Service lỗi"
                    });
                }
               


                return Content(body, "application/json");
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Không kết nối được Body Service"
                });

            }

        }

        [HttpPost]
        public async Task<IActionResult> StopCamera()
        {
            using var client = new HttpClient();

            try
            {
                await client.PostAsync("http://127.0.0.1:8000/stop-camera", null);
                await _logService.LogAsync(
                    GetCurrentUserId(),
                    "DỪNG CAMERA",
                    "Camera chấm công đã được tắt"
                );
                return Ok();
            }
            catch
            {
                await _logService.LogAsync(
     GetCurrentUserId(),
     "LỖI DỪNG CAMERA",
     "Không thể tắt camera"
 );

                return Ok();
            }
        }


        //In danh sách
        public async Task<IActionResult> Print(string? keyword, int? departmentId, DateTime? date, string? status)
        {
            var workDate = date ?? DateTime.Today;

            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(e =>
                    e.FullName.Contains(keyword) ||
                    e.EmployeeCode.Contains(keyword));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            var data = query
                .Select(e => new AttendanceRowViewModel
                {
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    DepartmentName = e.Department.DepartmentName,

                    CheckIn = _context.Attendances
                        .Where(a => a.EmployeeId == e.Id && a.WorkDate == workDate)
                        .Select(a => TimeOnly.FromDateTime(a.CheckTime))
                        .FirstOrDefault(),

                    Status = _context.Attendances.Any(a =>
                        a.EmployeeId == e.Id &&
                        a.WorkDate == workDate &&
                        a.Status == "Trễ")
                        ? "Đi trễ"
                        : _context.Attendances.Any(a =>
                            a.EmployeeId == e.Id &&
                            a.WorkDate == workDate)
                            ? "Đúng giờ"
                            : "Chưa chấm công"
                })
                .ToList();

            if (!string.IsNullOrEmpty(status))
            {
                data = data.Where(x => x.Status == status).ToList();
            }

            ViewBag.WorkDate = workDate;

            // Log
            await _logService.LogAsync(
      GetCurrentUserId(),
      "IN DANH SÁCH CHẤM CÔNG",
      $"Ngày in: {workDate:dd/MM/yyyy}"

);
            return View("Print", data);
        }


        private int GetCurrentUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );
        }




    }
}
