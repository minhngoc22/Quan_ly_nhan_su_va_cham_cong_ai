using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Models.ViewModels;


namespace QLNS.Controllers
{

    [Authorize(AuthenticationSchemes = "FaceIDAuth")]
    public class HomeController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public HomeController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ================== DATE ==================
            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;

            // ================== NHÂN VIÊN ==================
            int totalEmployees = _context.Employees.Count();

            int activeEmployees = _context.Employees
                .Count(e => e.Status == "?ang làm");

            int inactiveEmployees = totalEmployees - activeEmployees;

            int newEmployeesThisMonth = _context.Employees
                .Count(e => e.HireDate.HasValue
                    && e.HireDate.Value.Month == now.Month
                    && e.HireDate.Value.Year == now.Year);

            int employeesWithoutFace = _context.Employees
                .Count(e => !_context.FaceEmbeddings
                    .Any(f => f.EmployeeId == e.Id));

            // ================== USER + PHÒNG BAN ==================
            int totalUsers = _context.Users.Count();
            int totalDepartments = _context.Departments.Count();

            // ================== CH?M CÔNG ==================
            int todayAttendance = _context.Attendances
                .Count(a => a.WorkDate == today);

            int lateToday = _context.Attendances
                .Count(a => a.WorkDate == today && a.Status == "Tr?");

            int absentToday = Math.Max(0, activeEmployees - todayAttendance);

            // ================== LOG H? TH?NG ==================
            var recentActivities = _context.SystemLogs
      .Include(l => l.User)
          .ThenInclude(u => u.Employee)
              .ThenInclude(e => e.Position)
      .Where(l => l.User.UserRoles
          .Any(ur => ur.Role.RoleName == "HR"))
      .OrderByDescending(l => l.CreatedAt)
      .Take(5)
      .Select(l => new ActivityItemViewModel
      {
          Action = l.Action,

          UserName = l.User.Employee != null
              ? l.User.Employee.FullName
              : l.User.Username,

          Position =
              l.User.Employee != null &&
              l.User.Employee.Position != null
                  ? l.User.Employee.Position.PositionName
                  : "Hệ thống",

          CreatedAt = l.CreatedAt
      })
      .ToList();

            // ================== VIEW MODEL ==================
            var model = new HomeViewModel
            {
                TotalUsers = totalUsers,
                TotalEmployees = totalEmployees,
                TotalDepartments = totalDepartments,

                ActiveEmployees = activeEmployees,
                InactiveEmployees = inactiveEmployees,
                NewEmployeesThisMonth = newEmployeesThisMonth,

                TodayAttendance = todayAttendance,
                LateToday = lateToday,
                AbsentToday = absentToday,

                EmployeesWithoutFace = employeesWithoutFace,
                RecentActivities = recentActivities
            };

            return View(model);
        }

    }
}
