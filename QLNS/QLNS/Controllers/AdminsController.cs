using Microsoft.AspNetCore.Mvc;
using QLNS.Models;
using QLNS.Models.ViewModels;

namespace QLNS.Controllers
{
    public class AdminsController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public AdminsController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ================== DATE ==================
            var today = DateTime.Today;

            // ================== EMPLOYEE ==================
            int totalEmployees = _context.Employees.Count();

            int activeEmployees = _context.Employees
                .Count(e => e.Status == "Đang làm");

            int inactiveEmployees = totalEmployees - activeEmployees;

            // ================== ATTENDANCE ==================
            int todayAttendance = _context.Attendances
                .Count(a => a.CheckTime.Date == today);

            int lateToday = _context.Attendances
                .Count(a => a.Status == "Trễ"
                         && a.CheckTime.Date == today);

            int absentToday = Math.Max(0, activeEmployees - todayAttendance);

            // ================== FACE ID ==================
            int employeesWithoutFace = _context.Employees
                .Count(e => !_context.FaceEmbeddings
                    .Any(f => f.EmployeeId == e.Id));

            // ================== AI ALERT ==================
            int todayAlerts = _context.SecurityAlerts
                .Count(a => a.CreatedAt.HasValue &&
                            a.CreatedAt.Value.Date == today);

            int livenessFails = _context.SecurityAlerts
                .Count(a => a.AlertType == "LivenessFail"
                         && a.CreatedAt.HasValue
                         && a.CreatedAt.Value.Date == today);

            int unknownPersons = _context.MovementLogs
                .Count(m => m.PersonType == "Unknown"
                         && m.CreatedAt.HasValue
                         && m.CreatedAt.Value.Date == today);

            // ================== CAMERA ==================
            int activeCameras = _context.Cameras
                .Count(c => c.IsActive==true);

            // ================== SYSTEM LOG ==================
            var recentLogs = (
                from l in _context.SystemLogs
                join u in _context.Users
                    on l.UserId equals u.Id into lu
                from u in lu.DefaultIfEmpty()
                orderby l.CreatedAt descending
                select new SystemLogVM
                {
                    Action = l.Action,
                    Description = l.Description,
                    CreatedAt = l.CreatedAt,
                    UserName = u != null ? u.Username : "Hệ thống"
                })
                .Take(8)
                .ToList();

            // ================== VIEW MODEL ==================
            var vm = new AdminDashboardVM
            {
                // ===== KPI (8 ô) =====
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                TodayAttendance = todayAttendance,
                LateToday = lateToday,
                AbsentToday = absentToday,
                EmployeesWithoutFace = employeesWithoutFace,
                TodayAlerts = todayAlerts,
                ActiveCameras = activeCameras,

                // ===== AI =====
                UnknownPersons = unknownPersons,
                LivenessFails = livenessFails,

                // ===== LOG =====
                RecentLogs = recentLogs
            };

            return View(vm);
        }
        private List<int> GetWeeklyAttendance()
        {
            var result = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);

                var count = _context.Attendances
                    .Count(a => a.CheckTime.Date == day);

                result.Add(count);
            }

            return result;
        }
    }
}