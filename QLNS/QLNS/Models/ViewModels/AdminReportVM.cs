namespace QLNS.Models.ViewModels
{
    public class AdminReportVM
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveCameras { get; set; }

        public int TodayAttendance { get; set; }
        public int LateToday { get; set; }

        public int TodayAlerts { get; set; }
        public int LivenessFails { get; set; }
        public int FaceSpoofing { get; set; }
        public int UnknownPersons { get; set; }

        public List<TopUserRow> TopUsers { get; set; }
        public List<AlertTypeRow> AlertByType { get; set; }
    }

    public class TopUserRow
    {
        public string Username { get; set; }
        public int TotalActions { get; set; }
    }

    public class AlertTypeRow
    {
        public string AlertType { get; set; }
        public int Total { get; set; }
    }
}
