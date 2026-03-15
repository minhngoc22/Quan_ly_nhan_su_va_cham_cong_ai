namespace QLNS.Models.ViewModels
{
    public class AdminDashboardVM
    {
        // KPI
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TodayAttendance { get; set; }
        public int TodayAlerts { get; set; }
        public int ActiveCameras { get; set; }

        // NEW KPI
        public int LateToday { get; set; }
        public int AbsentToday { get; set; }
        public int EmployeesWithoutFace { get; set; }

        // AI
        public int UnknownPersons { get; set; }
        public int LivenessFails { get; set; }

        // ⭐ thêm
        public List<SystemLogVM> RecentLogs { get; set; } = new();
    }

    public class SystemLogVM
    {
        public string Action { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}