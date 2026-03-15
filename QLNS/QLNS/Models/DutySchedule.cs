using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class DutySchedule
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int DutyShiftId { get; set; }

    public DateTime DutyDate { get; set; }

    public virtual DutyShift DutyShift { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
