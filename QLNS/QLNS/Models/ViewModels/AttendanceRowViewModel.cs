namespace QLNS.Models.ViewModels
{
    public class AttendanceRowViewModel
    {
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }

        public TimeOnly? CheckIn { get; set; }
        public TimeOnly? CheckOut { get; set; }

        public string Status { get; set; }

        
    }

    public class AttendanceFilterViewModel
    {
        public string? Keyword { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? Date { get; set; }

        // 👉 THÊM
        public string? Status { get; set; }
        public List<AttendanceRowViewModel> Data { get; set; } = new();
    }


}
