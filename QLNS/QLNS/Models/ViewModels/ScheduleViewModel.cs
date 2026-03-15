namespace QLNS.Models.ViewModels
{
    public class ScheduleViewModel
    {
        public int ScheduleId { get; set; }

        public string EmployeeName { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public int ShiftId { get; set; }
        public string ShiftName { get; set; }


        public DateTime WorkDate { get; set; }   // ⚠ KHÔNG dùng DateOnly
        public string EmployeeStatus { get; set; }

        public bool IsOnLeave { get; set; }
        public string LeaveType { get; set; }

        public bool HasAttendance { get; set; }

        public decimal OTHours { get; set; }



        // ===== DUTY
        public bool HasDuty { get; set; }
        public string DutyName { get; set; }
        public bool HasDutyAttendance { get; set; }
        public string DutyStatus { get; set; }

        //leave
        public bool IsCompanyLeave { get; set; }

    }

    public class ScheduleCreateVM
    {
        public int EmployeeId { get; set; }
        public int ShiftId { get; set; }
        public DateTime WorkDate { get; set; }
    }

}
