using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLNS.Models;
using QLNS.Models.ViewModels;
using System.Linq;

namespace QLNS.Controllers
{
    [Authorize(Roles = "Employee")]
    public class StaffController : Controller
    {
        private readonly FaceIdHrmsContext _db;

        public StaffController(FaceIdHrmsContext db)
        {
            _db = db;
        }

        // Trang cá nhân
        public IActionResult Index()
        {
            var now = DateTime.Now;
            var username = User.Identity!.Name;

            // ================== PROFILE ==================
            var profile = (
                from u in _db.Users
                join e in _db.Employees on u.EmployeeId equals e.Id
                join d in _db.Departments on e.DepartmentId equals d.Id
                join p in _db.Positions on e.PositionId equals p.Id
                join ex in _db.ExperienceLevels
                    on e.ExperienceLevelId equals ex.Id into exGroup
                from ex in exGroup.DefaultIfEmpty()
                where u.Username == username
                select new StaffProfileViewModel
                {
                    EmployeeCode = e.EmployeeCode,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = d.DepartmentName,

                    FullName = e.FullName,
                    Gender = e.Gender,
                    DateOfBirth = e.DateOfBirth,

                    Email = e.Email,
                    Phone = e.Phone,
                    Address = e.Address,

                    PositionId = e.PositionId,
                    PositionName = p.PositionName,

                    ExperienceLevelId = e.ExperienceLevelId,
                    ExperienceLevelName = ex != null ? ex.LevelName : "Chưa có",

                    Status = e.Status,
                    Avatar = e.Avatar
                }
            ).FirstOrDefault();

            if (profile == null)
                return Unauthorized();

            // Lấy EmployeeId để dùng tiếp
            var employeeId = (
                from u in _db.Users
                where u.Username == username
                select u.EmployeeId
            ).First();

            // ================== SCHEDULE + ATTENDANCE ==================
            var schedules = _db.WorkSchedules
                .Where(x => x.EmployeeId == employeeId
                    && x.WorkDate.HasValue
                    && x.WorkDate.Value.Month == now.Month
                    && x.WorkDate.Value.Year == now.Year)
                .ToList();

            var attendances = _db.Attendances
                .Where(x => x.EmployeeId == employeeId
                    && x.WorkDate.HasValue
                    && x.WorkDate.Value.Month == now.Month
                    && x.WorkDate.Value.Year == now.Year)
                .ToList();

            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var calendar = new List<CalendarDayViewModel>();

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(now.Year, now.Month, d);

                var total = schedules.Count(x => x.WorkDate!.Value.Date == date);
                var worked = attendances.Count(x => x.WorkDate!.Value.Date == date);

                // 👇👇👇 SỬA ĐOẠN NÀY
                string status;

                if (date.Date > DateTime.Today)
                {
                    status = "white";
                }
                else if (total == 0)
                {
                    status = "white";
                }
                else if (worked >= 2)
                {
                    status = "green";   // ✅ đủ 2 ca sáng + chiều
                }
                else if (worked == 1)
                {
                    status = "yellow";  // ✅ làm 1 ca
                }
                else
                {
                    status = "red";     // ✅ có lịch nhưng không làm
                }

                calendar.Add(new CalendarDayViewModel
                {
                    Date = date,
                    IsFuture = date.Date > DateTime.Today,
                    TotalShifts = total,
                    WorkedShifts = worked,
                    Status = status
                });
            }

            var schedule = new MyScheduleViewModel
            {
                Month = now.Month,
                Year = now.Year,
                Days = calendar
            };

            // ================== GỘP ==================
            var vm = new StaffDashboardViewModel
            {
                Profile = profile,
                Schedule = schedule
            };

            return View(vm);
        }


        public IActionResult MySchedule(int? month, int? year, string type = "WORK")
        {
            var now = DateTime.Now;
            month ??= now.Month;
            year ??= now.Year;

            var username = User.Identity!.Name;

            var employeeId = _db.Users
                .Where(u => u.Username == username)
                .Select(u => u.EmployeeId)
                .First();

            List<CalendarDayViewModel> calendar;

            // ================= LỊCH LÀM =================
            if (type == "WORK")
            {
                var schedules = _db.WorkSchedules
                    .Where(x => x.EmployeeId == employeeId
                        && x.WorkDate.HasValue
                        && x.WorkDate.Value.Month == month
                        && x.WorkDate.Value.Year == year)
                    .ToList();

                var attendances = _db.Attendances
                    .Where(x => x.EmployeeId == employeeId
                        && x.WorkDate.HasValue
                        && x.WorkDate.Value.Month == month
                        && x.WorkDate.Value.Year == year)
                    .ToList();

                calendar = BuildWorkCalendar(schedules, attendances, year.Value, month.Value);
            }
            // ================= LỊCH TRỰC =================
            else if (type == "DUTY")
            {
                var duties = _db.DutySchedules
                    .Where(x => x.EmployeeId == employeeId
                        && x.DutyDate.Month == month
                        && x.DutyDate.Year == year)
                    .ToList();

                var dutyAttendances = _db.DutyAttendances
                    .Where(x => x.EmployeeId == employeeId
                        && x.DutyDate.Month == month
                        && x.DutyDate.Year == year)
                    .ToList();

                calendar = BuildDutyCalendar(duties, dutyAttendances, year.Value, month.Value);
            }

            // ✅ ===== LEAVE =====
            else
            {
                var startMonth = new DateTime(year.Value, month.Value, 1);
                var endMonth = startMonth.AddMonths(1).AddDays(-1);

                var leaves = _db.LeaveRequests
                    .Where(x => x.EmployeeId == employeeId
                        && x.Status == "Duyệt"
                        && x.FromDate <= endMonth
                        && x.ToDate >= startMonth)
                    .ToList();

                calendar = BuildLeaveCalendar(leaves, year.Value, month.Value);
            }

            return View(new MyScheduleViewModel
            {
                Month = month.Value,
                Year = year.Value,
                Type = type,
                Days = calendar
            });
            
        }


        //================= lịch trực =================
        private List<CalendarDayViewModel> BuildWorkCalendar(
    List<WorkSchedule> schedules,
    List<Attendance> attendances,
    int year,
    int month)
        {
            var days = new List<CalendarDayViewModel>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(year, month, d);
                bool isFuture = date.Date > DateTime.Today;

                bool hasSchedule = schedules.Any(x => x.WorkDate!.Value.Date == date);
                int total = hasSchedule ? 2 : 0;
                int worked = attendances.Count(x => x.WorkDate!.Value.Date == date);

                days.Add(BuildDay(date, total, worked, isFuture, false));

            }

            return days;
        }


        private List<CalendarDayViewModel> BuildDutyCalendar(
    List<DutySchedule> duties,
    List<DutyAttendance> attendances,
    int year,
    int month)
        {
            var days = new List<CalendarDayViewModel>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(year, month, d);
                bool isFuture = date.Date > DateTime.Today;

                int total = duties.Count(x => x.DutyDate.Date == date);
                int worked = attendances.Count(x => x.DutyDate.Date == date);

                days.Add(BuildDay(date, total, worked, isFuture, true));

            }

            return days;
        }

        private List<CalendarDayViewModel> BuildLeaveCalendar(
    List<LeaveRequest> leaves,
    int year,
    int month)
        {
            var days = new List<CalendarDayViewModel>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(year, month, d);
                bool isFuture = date.Date > DateTime.Today;

                bool isLeave = leaves.Any(l =>
                    l.FromDate.Date <= date &&
                    l.ToDate.Date >= date);

                string status = isLeave ? "blue" : "white";

                days.Add(new CalendarDayViewModel
                {
                    Date = date,
                    IsFuture = isFuture,
                    TotalShifts = isLeave ? 1 : 0,
                    WorkedShifts = 0,
                    Status = status
                });
            }

            return days;
        }
        private CalendarDayViewModel BuildDay(DateTime date, int total, int worked, bool isFuture, bool isDuty)
        {
            string status;

            if (isDuty)
            {
                // ===== LỊCH TRỰC =====
                if (total == 0)
                {
                    status = "white";
                }
                else if (isFuture)
                {
                    status = "yellow"; // 🔥 có lịch trực nhưng chưa tới
                }
                else
                {
                    status = worked >= 1 ? "green" : "red"; // 🔥 1 ca là đủ
                }
            }
            else
            {
                // ===== LỊCH LÀM =====
                if (isFuture || total == 0)
                    status = "white";
                else if (worked >= total)
                    status = "green";
                else if (worked > 0)
                    status = "yellow";
                else
                    status = "red";
            }

            return new CalendarDayViewModel
            {
                Date = date,
                IsFuture = isFuture,
                TotalShifts = total,
                WorkedShifts = worked,
                Status = status
            };

        }

        [HttpGet]
        public IActionResult Profile()
        {
            var username = User.Identity!.Name;

            var vm = (from u in _db.Users
                     join e in _db.Employees on u.EmployeeId equals e.Id
                     join d in _db.Departments on e.DepartmentId equals d.Id
                     join p in _db.Positions on e.PositionId equals p.Id
                     join ex in _db.ExperienceLevels
                         on e.ExperienceLevelId equals ex.Id into exGroup
                     from ex in exGroup.DefaultIfEmpty()
                     where u.Username == username
                     select new StaffProfileViewModel
                     {
                         EmployeeCode = e.EmployeeCode,
                         DepartmentId = e.DepartmentId,
                         DepartmentName = d.DepartmentName,

                         FullName = e.FullName,
                         Gender = e.Gender,
                         DateOfBirth = e.DateOfBirth,

                         Email = e.Email,
                         Phone = e.Phone,
                         Address = e.Address,

                         PositionId = e.PositionId,
                         PositionName = p.PositionName,

                         ExperienceLevelId = e.ExperienceLevelId,
                         ExperienceLevelName = ex != null ? ex.LevelName : "Chưa có",

                         Status = e.Status,
                         Avatar = e.Avatar
                     }
            ).FirstOrDefault();

            if (vm == null)
                return Unauthorized();

            return View(vm);
        }


        // Kiểm tra xem hash có phải là BCrypt không
        private bool IsBCryptHash(string hash)
        {
            return hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$");
        }


        // Trang đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Profile");

            var username = User.Identity!.Name;
            var user = _db.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
                return Unauthorized();

            bool isValidPassword = false;

            // ===== TRƯỜNG HỢP 1: password đã là BCrypt =====
            if (IsBCryptHash(user.PasswordHash))
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(
                    model.CurrentPassword,
                    user.PasswordHash
                );
            }
            // ===== TRƯỜNG HỢP 2: password cũ (plain text) =====
            else
            {
                isValidPassword = model.CurrentPassword == user.PasswordHash;

                // 👉 migrate sang BCrypt
                if (isValidPassword)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.CurrentPassword);
                    _db.SaveChanges();
                }
            }

            if (!isValidPassword)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng";
                return RedirectToAction("Profile");
            }

            // ===== HASH mật khẩu mới =====
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công";
            return RedirectToAction("Profile");
        }




    }
}
