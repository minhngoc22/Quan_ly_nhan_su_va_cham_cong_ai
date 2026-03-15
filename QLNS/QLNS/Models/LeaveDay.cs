using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class LeaveDay
{
    public int Id { get; set; }

    public int LeaveRequestId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime LeaveDate { get; set; }

    public string? SessionType { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LeaveRequest LeaveRequest { get; set; } = null!;
}
