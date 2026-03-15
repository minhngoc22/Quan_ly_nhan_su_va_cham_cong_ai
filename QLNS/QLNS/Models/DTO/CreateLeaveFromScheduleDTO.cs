namespace QLNS.Models.DTO
{
    public class CreateLeaveFromScheduleDTO
    {
        public int? ScheduleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Session { get; set; } // FULL / MORNING / AFTERNOON
        public string LeaveType { get; set; }
    }

    public class CreateCompanyLeaveDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string LeaveType { get; set; }
    }
}
