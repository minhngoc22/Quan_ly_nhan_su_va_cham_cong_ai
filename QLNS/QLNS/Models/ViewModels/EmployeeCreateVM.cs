using System.ComponentModel.DataAnnotations;

namespace QLNS.Models.ViewModels
{
    public class EmployeeCreateVM
    {
        //[Required]
        public string EmployeeCode { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int PositionId { get; set; }

        public int? ExperienceLevelId { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public DateTime? HireDate { get; set; }

        public string Status { get; set; } = "Đang làm";
    }
}
