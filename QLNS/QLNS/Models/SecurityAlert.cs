using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class SecurityAlert
{
    public int Id { get; set; }

    public int CameraId { get; set; }

    public int? EmployeeId { get; set; }

    public string? AlertType { get; set; }

    public string? Description { get; set; }

    public int? OccurrenceCount { get; set; }

    public bool? IsSentToDiscord { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Camera Camera { get; set; } = null!;

    public virtual Employee? Employee { get; set; }
}
