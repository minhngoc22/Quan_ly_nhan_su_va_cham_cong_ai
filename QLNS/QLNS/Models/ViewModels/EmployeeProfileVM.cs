namespace QLNS.Models.ViewModels
{
    public class EmployeeProfileVM
    {
        public int Id { get; set; }

        // ===== KHÓA / KHÔNG SỬA =====
        public string EmployeeCode { get; set; }
        public int DepartmentId { get; set; }

        // ===== CÓ THỂ SỬA =====
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }   // ❌ DateOnly -> ✅ DateTime


        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public int PositionId { get; set; }
        public int? ExperienceLevelId { get; set; }

        public string Status { get; set; }
        public string Avatar { get; set; }
    }
}
