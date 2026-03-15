namespace QLNS.Models.DTO
{
    public class GenerateScheduleRequest
    {
        public string Month { get; set; }      // yyyy-MM
        public int? DepartmentId { get; set; }
        public string ShiftType { get; set; }  // Morning | Afternoon | Both
    }


}
