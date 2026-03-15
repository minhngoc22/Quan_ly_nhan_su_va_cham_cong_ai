namespace QLNS.Models.ViewModels
{
    public class ScheduleDayGroupVM
    {
        public DateTime WorkDate { get; set; }
        public List<ScheduleViewModel> Items { get; set; } = new();
    }

}
