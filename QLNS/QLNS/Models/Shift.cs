using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Shift
{
    public int Id { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public TimeOnly LateThreshold { get; set; }

    public bool IsActive { get; set; }

    public bool IsAttendanceOpen { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool AllowAttendance { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<EarlyCheckinLog> EarlyCheckinLogs { get; set; } = new List<EarlyCheckinLog>();

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
