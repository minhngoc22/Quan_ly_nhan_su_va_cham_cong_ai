using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class DutyAttendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int DutyShiftId { get; set; }

    public DateTime CheckTime { get; set; }

    public DateTime DutyDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual DutyShift DutyShift { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
