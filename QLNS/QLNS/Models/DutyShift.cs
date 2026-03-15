using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class DutyShift
{
    public int Id { get; set; }

    public string? DutyName { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public bool? IsOvernight { get; set; }

    public bool IsActive { get; set; }

    public bool AllowAttendance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<DutyAttendance> DutyAttendances { get; set; } = new List<DutyAttendance>();

    public virtual ICollection<DutySchedule> DutySchedules { get; set; } = new List<DutySchedule>();
}
