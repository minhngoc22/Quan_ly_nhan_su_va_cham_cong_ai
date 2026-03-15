using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int ShiftId { get; set; }

    public DateTime CheckTime { get; set; }

    public DateTime? WorkDate { get; set; }

    public string? Status { get; set; }

    public double? SimilarityScore { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;
}
