using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Camera
{
    public int Id { get; set; }

    public string CameraCode { get; set; } = null!;

    public string? CameraName { get; set; }

    public string? Location { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<EarlyCheckinLog> EarlyCheckinLogs { get; set; } = new List<EarlyCheckinLog>();

    public virtual ICollection<MovementLog> MovementLogs { get; set; } = new List<MovementLog>();

    public virtual ICollection<SecurityAlert> SecurityAlerts { get; set; } = new List<SecurityAlert>();
}
