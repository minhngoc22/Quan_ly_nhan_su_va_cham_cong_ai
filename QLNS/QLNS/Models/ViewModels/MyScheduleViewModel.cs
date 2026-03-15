namespace QLNS.Models.ViewModels
{
    public class MyScheduleViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Type { get; set; } = "WORK"; // WORK | DUTY
        public List<CalendarDayViewModel> Days { get; set; } = new();
    }


    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsFuture { get; set; }

        public int TotalShifts { get; set; }
        public int WorkedShifts { get; set; }

        public string Status { get; set; } // Green / Yellow / Red / White
    }


    public class StaffProfileViewModel
    {
        // ===== KHÓA / KHÔNG SỬA =====
        public string EmployeeCode { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        // ===== CÓ THỂ SỬA =====
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public int? PositionId { get; set; }
        public string PositionName { get; set; }

        public int? ExperienceLevelId { get; set; }
        public string ExperienceLevelName { get; set; }

        public string Status { get; set; }
        public string Avatar { get; set; }
    }
}
