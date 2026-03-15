namespace QLNS.Models.ViewModels
{
    public class HomeViewModel
    {
        // ===== KPI CHUNG =====
        public int TotalUsers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }

        // ===== NHÂN VIÊN =====
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public int NewEmployeesThisMonth { get; set; }

        // ===== CHẤM CÔNG =====
        public int TodayAttendance { get; set; }
        public int AbsentToday { get; set; }
        public int LateToday { get; set; }

        // ===== FACE ID =====
        public int EmployeesWithoutFace { get; set; }

        // ===== LOG =====
        public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
    }

    public class ActivityItemViewModel
    {
        public string Action { get; set; }
        public string UserName { get; set; }
        public string Position { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
