using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class WorkSchedule
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public int? ShiftId { get; set; }

    public DateTime? WorkDate { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Shift? Shift { get; set; }
}
