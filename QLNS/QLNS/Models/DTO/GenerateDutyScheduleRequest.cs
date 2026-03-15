namespace QLNS.Models.DTO
{
    public class GenerateDutyScheduleRequest
    {
        public int DepartmentId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int DutyShiftId { get; set; }
        public string Rule { get; set; } // ROTATE | FIXED
    }

    public class DutyPreviewVM
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime DutyDate { get; set; }

        public int DutyShiftId { get; set; }
        public string DutyShiftName { get; set; }
    }



}
