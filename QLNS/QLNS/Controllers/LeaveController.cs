using Microsoft.AspNetCore.Mvc;
using QLNS.Models.DTO;
using QLNS.Models;
using QLNS.Services;
using Microsoft.EntityFrameworkCore;

namespace QLNS.Controllers
{
    [ApiController] // ⭐ QUAN TRỌNG (nhận JSON từ fetch)
    [Route("[controller]")]
    public class LeaveController : Controller
    {
        private readonly FaceIdHrmsContext _db;
        private readonly LogService _logService;

        public LeaveController(FaceIdHrmsContext db, LogService logService)
        {
            _db = db;
            _logService = logService;
        }

        // ===============================
        // TẠO NGHỈ PHÉP TỪ SCHEDULE
        // ===============================
        [HttpPost("CreateFromSchedule")]
        public async Task<IActionResult> CreateFromSchedule(
     [FromBody] CreateLeaveFromScheduleDTO dto)
        {
            if (dto == null || dto.ScheduleId == null)
                return BadRequest(new { success = false });

            var schedule = await _db.WorkSchedules.FindAsync(dto.ScheduleId);

            if (schedule == null || !schedule.EmployeeId.HasValue)
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy nhân viên"
                });

            DateTime from;
            DateTime to;

            // ================= NỬA NGÀY =================
            if (dto.Session == "MORNING")
            {
                from = dto.FromDate.Date.AddHours(8);   // 08:00
                to = dto.FromDate.Date.AddHours(12);  // 12:00
            }
            else if (dto.Session == "AFTERNOON")
            {
                from = dto.FromDate.Date.AddHours(13);  // 13:00
                to = dto.FromDate.Date.AddHours(17);  // 17:00
            }
            else // FULL DAY / MULTI DAY
            {
                from = dto.FromDate.Date;
                to = dto.ToDate.Date
                            .AddHours(23)
                            .AddMinutes(59);
            }

            // ================= CREATE =================
            var leave = new LeaveRequest
            {
                EmployeeId = schedule.EmployeeId.Value,
                FromDate = from,
                ToDate = to,
                LeaveType = dto.LeaveType,
                Status = "Duyệt",
                CreatedAt = DateTime.Now
            };

            _db.LeaveRequests.Add(leave);
            await _db.SaveChangesAsync();

            // ================= LOG =================
            await _logService.LogAsync(
                leave.EmployeeId,
                "CREATE_LEAVE",
                $"Tạo nghỉ phép từ {leave.FromDate:dd/MM/yyyy HH:mm} " +
                $"đến {leave.ToDate:dd/MM/yyyy HH:mm}"
            );

            return Ok(new { success = true });
        }

        // ===============================  CHO CÔNG TY NGHỈ PHÉP  ===============================
        [HttpPost("CreateCompanyLeave")]
        public async Task<IActionResult> CreateCompanyLeave([FromBody] CreateCompanyLeaveDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { success = false });

                if (dto.FromDate > dto.ToDate)
                    return BadRequest(new { success = false });

                DateTime from = dto.FromDate.Date;
                DateTime to = dto.ToDate.Date.AddHours(23).AddMinutes(59);

                var employees = await _db.Employees.Select(e => e.Id).ToListAsync();

                if (!employees.Any())
                    return Ok(new { success = false, message = "Không có nhân viên nào" });

                var leaves = new List<LeaveRequest>();

                foreach (var empId in employees)
                {
                    bool existed = await _db.LeaveRequests.AnyAsync(l =>
                        l.EmployeeId == empId &&
                        l.FromDate <= to &&
                        l.ToDate >= from &&
                        (l.IsCompanyLeave ?? false) == true);

                    if (existed) continue;

                    var workSchedules = await _db.WorkSchedules
                        .Where(w => w.EmployeeId == empId &&
                               w.WorkDate >= from &&
                               w.WorkDate <= to)
                        .ToListAsync();

                    if (workSchedules.Any())
                        _db.WorkSchedules.RemoveRange(workSchedules);

                    var dutySchedules = await _db.DutySchedules
                        .Where(d => d.EmployeeId == empId &&
                               d.DutyDate >= from &&
                               d.DutyDate <= to)
                        .ToListAsync();

                    if (dutySchedules.Any())
                        _db.DutySchedules.RemoveRange(dutySchedules);

                    leaves.Add(new LeaveRequest
                    {
                        EmployeeId = empId,
                        FromDate = from,
                        ToDate = to,
                        LeaveType = dto.LeaveType,
                        Status = "Duyệt",
                        IsCompanyLeave = true,
                        CreatedAt = DateTime.Now
                    });
                }

                if (!leaves.Any())
                    return Ok(new { success = false, message = "Tất cả nhân viên đã có lịch nghỉ" });

                await _db.LeaveRequests.AddRangeAsync(leaves);
                await _db.SaveChangesAsync();

                try
                {
                    await _logService.LogAsync(
                        0,
                        "CREATE_COMPANY_LEAVE",
                        $"Nghỉ toàn công ty {from:dd/MM/yyyy} - {to:dd/MM/yyyy}"
                    );
                }
                catch { }

                return Ok(new { success = true, count = leaves.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}