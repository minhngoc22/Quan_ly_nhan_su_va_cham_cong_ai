using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string? LeaveType { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsCompanyLeave { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
