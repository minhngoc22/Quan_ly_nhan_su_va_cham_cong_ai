using System.ComponentModel.DataAnnotations;

namespace QLNS.Models.ViewModels
{
    public class ShiftVM
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public TimeOnly LateThreshold { get; set; }

        public bool IsActive { get; set; }
        public bool AllowAttendance { get; set; }
        public bool IsAttendanceOpen { get; set; }

        public string Type { get; set; } // WORK | DUTY
    }
}