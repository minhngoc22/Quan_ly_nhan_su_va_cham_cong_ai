using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNS.Models;
using QLNS.Models.ViewModels;
using QLNS.Models.DTO;
using System;
using System.Linq;
using QLNS.Services;
using System.Security.Claims;

namespace QLNS.Controllers
{
    [Authorize(AuthenticationSchemes = "FaceIDAuth")]
    public class ScheduleController : Controller
    {
        private readonly FaceIdHrmsContext _db;
        private readonly LogService _logService; // ⭐ THÊM

        public ScheduleController(FaceIdHrmsContext db, LogService logService)
        {
            _db = db;
            _logService = logService; // ⭐ THÊM
        }

        public IActionResult Index(
     string keyword,
     int? departmentId,
     int? shiftId,
     DateTime? fromDate,
     DateTime? toDate,
     string shiftType = "WORK",
     int page = 1,
     int pageSize = 10
 )
        {
            // ================= VIEWBAG =================
            ViewBag.Departments = _db.Departments.ToList();
            ViewBag.Shifts = _db.Shifts.ToList();
            ViewBag.ShiftType = shiftType;

            IQueryable<ScheduleViewModel> query;

            // =====================================================
            // ================= WORK TAB ==========================
            // =====================================================
            if (shiftType == "WORK")
            {
                query =
                    from ws in _db.WorkSchedules
                    join e in _db.Employees on ws.EmployeeId equals e.Id
                    join d in _db.Departments on e.DepartmentId equals d.Id
                    join s in _db.Shifts on ws.ShiftId equals s.Id

                    let leave = _db.LeaveRequests
                        .Where(l =>
                            l.EmployeeId == ws.EmployeeId &&
                            l.Status == "Duyệt" &&
                            ws.WorkDate >= l.FromDate &&
                            ws.WorkDate <= l.ToDate
                        )
                        .Select(l => l.LeaveType)
                        .FirstOrDefault()

                    let attendanceStatus = _db.Attendances
                        .Where(a =>
                            a.EmployeeId == ws.EmployeeId &&
                            a.ShiftId == ws.ShiftId &&
                            a.CheckTime >= ws.WorkDate &&
                            a.CheckTime < ws.WorkDate.Value.AddDays(1)
                        )
                        .Select(a => a.Status)
                        .FirstOrDefault()

                    select new ScheduleViewModel
                    {
                        ScheduleId = ws.Id,
                        EmployeeName = e.FullName,
                        EmployeeStatus = e.Status,

                        DepartmentId = d.Id,
                        DepartmentName = d.DepartmentName,

                        ShiftId = s.Id,
                        ShiftName = s.ShiftName,

                        WorkDate = ws.WorkDate ?? DateTime.MinValue,

                        IsOnLeave = leave != null,
                        LeaveType = leave,

                        HasAttendance = attendanceStatus != null,
                    

                        HasDuty = false
                    };
            }

            // =====================================================
            // ================= DUTY TAB ==========================
            // =====================================================
            else if (shiftType == "DUTY")
            {
                query =
                    from ds in _db.DutySchedules
                    join e in _db.Employees on ds.EmployeeId equals e.Id
                    join d in _db.Departments on e.DepartmentId equals d.Id
                    join sh in _db.DutyShifts on ds.DutyShiftId equals sh.Id

                    let dutyAttendance = _db.DutyAttendances
                        .Where(da =>
                            da.EmployeeId == ds.EmployeeId &&
                            da.DutyShiftId == ds.DutyShiftId &&
                            da.DutyDate == ds.DutyDate
                        )
                        .Select(da => da.Status)
                        .FirstOrDefault()

                    select new ScheduleViewModel
                    {
                        ScheduleId = ds.Id,
                        EmployeeName = e.FullName,
                        EmployeeStatus = e.Status,

                        DepartmentId = d.Id,
                        DepartmentName = d.DepartmentName,

                        WorkDate = ds.DutyDate,

                        HasDuty = true,
                        DutyName = sh.DutyName,
                        HasDutyAttendance = dutyAttendance != null,
                        DutyStatus = dutyAttendance
                    };
            }

            // =====================================================
            // ================= LEAVE TAB =========================
            // =====================================================
            else // LEAVE
            {
                query =
                    from l in _db.LeaveRequests
                    join e in _db.Employees on l.EmployeeId equals e.Id
                    join d in _db.Departments on e.DepartmentId equals d.Id
                    select new ScheduleViewModel
                    {
                        ScheduleId = l.Id,
                        EmployeeName = e.FullName,
                        EmployeeStatus = e.Status,

                        DepartmentId = d.Id,
                        DepartmentName = d.DepartmentName,

                        WorkDate = l.FromDate,

                        LeaveType = l.LeaveType,
                        IsOnLeave = true,
                        IsCompanyLeave = l.IsCompanyLeave ?? false
                    };
            }

            //
            // ✅ FILTER DÙNG CHUNG (ĐẶT NGOÀI)
            //
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.EmployeeName.Contains(keyword));

            if (departmentId.HasValue)
                query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (fromDate.HasValue)
                query = query.Where(x => x.WorkDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.WorkDate <= toDate.Value.Date);

            // =====================================================
            // ================= PAGINATION ========================
            // =====================================================

            int totalItems = query.Count();

            var data = query
                .OrderBy(x => x.WorkDate)
                .ThenBy(x => x.EmployeeName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(data);
        }


        // ================= TẠO LỊCH LÀM THEO THÁNG =================

        [HttpPost]
        public async Task<IActionResult> GenerateMonth([FromBody] GenerateScheduleRequest req)

        {
            if (string.IsNullOrEmpty(req.Month))
                return BadRequest("Tháng không hợp lệ");

            // ================= PARSE THÁNG =================
            var monthDate = DateTime.Parse(req.Month + "-01");
            var nowMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // ❌ không cho tạo tháng quá khứ
            if (monthDate < nowMonth)
                return BadRequest("Không thể tạo lịch cho tháng quá khứ");

            var start = new DateTime(monthDate.Year, monthDate.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            // ================= NHÂN VIÊN =================
            var employees = _db.Employees
                .Where(e => e.Status == "Đang làm")
                .AsQueryable();

            if (req.DepartmentId.HasValue)
            {
                employees = employees
                    .Where(e => e.DepartmentId == req.DepartmentId);
            }

            var employeeList = employees.ToList();

            // ================= CA LÀM =================
            // Giả sử:
            // 1 = Sáng
            // 2 = Chiều

            var shiftsToCreate = new List<int>();

            if (req.ShiftType == "Morning")
                shiftsToCreate.Add(1);
            else if (req.ShiftType == "Afternoon")
                shiftsToCreate.Add(2);
            else
            {
                // Không chọn ca → tạo cả 2
                shiftsToCreate.Add(1);
                shiftsToCreate.Add(2);
            }

            // ================= TẠO LỊCH =================
            foreach (var emp in employeeList)
            {
                for (var day = start; day <= end; day = day.AddDays(1))
                {
                    // ❌ Bỏ Chủ nhật
                    if (day.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    foreach (var shiftId in shiftsToCreate)
                    {
                        bool exists = _db.WorkSchedules.Any(w =>
                            w.EmployeeId == emp.Id &&
                            w.WorkDate == day &&
                            w.ShiftId == shiftId);

                        if (exists) continue;

                        _db.WorkSchedules.Add(new WorkSchedule
                        {
                            EmployeeId = emp.Id,
                            ShiftId = shiftId,
                            WorkDate = day,
                           
                        });
                    }
                }
            }

            _db.SaveChanges();

            // ✅ GHI LOG (GIỐNG TẠO LỊCH TRỰC)
            await _logService.LogAsync(
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                "Tạo lịch làm",
                $"Tháng={req.Month}, PhòngBan={req.DepartmentId ?? 0}, Ca={req.ShiftType ?? "ALL"}"
            );

            return Json(new
            {
                success = true,
                message = "Tạo lịch thành công"
            });

        }


        // ================= TẠO LỊCH TRỰC =================

        [HttpPost]
        public IActionResult PreviewDuty([FromBody] GenerateDutyScheduleRequest req)
        {
            var employees = _db.Employees
                .Where(e => e.Status == "Đang làm"
                    && e.DepartmentId == req.DepartmentId)
                .OrderBy(e => e.Id)
                .ToList();

            if (!employees.Any())
                return BadRequest("Phòng ban không có nhân viên");

            var result = new List<DutyPreviewVM>();
            int index = 0;

            var dutyShifts = _db.DutyShifts
                .Where(x => x.Id == 1 || x.Id == 2) // sáng + tối
                .OrderBy(x => x.Id)
                .ToList();

            for (var day = req.FromDate.Date; day <= req.ToDate.Date; day = day.AddDays(1))
            {
                if (day.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                foreach (var shift in dutyShifts)
                {
                    Employee emp;

                    if (req.Rule == "ROTATE")
                    {
                        emp = employees[index % employees.Count];
                        index++;
                    }
                    else
                    {
                        emp = employees.First(); // FIXED
                    }

                    result.Add(new DutyPreviewVM
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.FullName,
                        DutyDate = day,
                        DutyShiftId = shift.Id,
                        DutyShiftName = shift.DutyName
                    });
                }
            }

            return Json(result);
        }

        // ================= LƯU LỊCH TRỰC =================
        [HttpPost]
        public async Task<IActionResult> GenerateDutySchedule([FromBody] List<DutyPreviewVM> list)
        {
            if (list == null || !list.Any())
                return BadRequest("Danh sách lịch trực rỗng");

            int createdCount = 0;

            foreach (var item in list)
            {
                bool exists = _db.DutySchedules.Any(d =>
                    d.EmployeeId == item.EmployeeId &&
                    d.DutyDate == item.DutyDate &&
                    d.DutyShiftId == item.DutyShiftId);

                if (exists) continue;

                _db.DutySchedules.Add(new DutySchedule
                {
                    EmployeeId = item.EmployeeId,
                    DutyDate = item.DutyDate,
                    DutyShiftId = item.DutyShiftId
                });

                createdCount++;
            }

            await _db.SaveChangesAsync();

            await _logService.LogAsync(
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                "Tạo lịch trực ",
                $"Total={createdCount}"
            );

            return Json(new
            {
                success = true,
                message = $"Tạo lịch trực thành công ({createdCount} ca)"
            });
        }



        [HttpDelete]
        public async Task<IActionResult> DeleteWork(int id)
        {
            var schedule = await _db.WorkSchedules.FindAsync(id);

            if (schedule == null)
                return NotFound(new { success = false });

            _db.WorkSchedules.Remove(schedule);
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDuty(int id)
        {
            var duty = await _db.DutySchedules.FindAsync(id);

            if (duty == null)
                return NotFound(new { success = false });

            _db.DutySchedules.Remove(duty);
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leave = await _db.LeaveRequests.FindAsync(id);

            if (leave == null)
                return NotFound(new { success = false });

            _db.LeaveRequests.Remove(leave);
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

    }
}
