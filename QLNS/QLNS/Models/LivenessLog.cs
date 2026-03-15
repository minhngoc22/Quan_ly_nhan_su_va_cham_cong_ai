using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class LivenessLog
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public bool? BlinkDetected { get; set; }

    public bool? HeadMovement { get; set; }

    public bool? Result { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee? Employee { get; set; }
}
