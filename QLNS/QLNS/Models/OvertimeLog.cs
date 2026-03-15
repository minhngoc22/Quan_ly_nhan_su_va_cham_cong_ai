using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class OvertimeLog
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public decimal? Hours { get; set; }

    public string? Ottype { get; set; }

    public int? ApprovedBy { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
