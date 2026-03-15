using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class MovementLog
{
    public int Id { get; set; }

    public int CameraId { get; set; }

    public string? PersonType { get; set; }

    public int? EmployeeId { get; set; }

    public double? FaceSimilarity { get; set; }

    public double? BodySimilarity { get; set; }

    public byte[]? SnapshotImage { get; set; }

    public string? SnapshotPath { get; set; }

    public string? TrackingId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Camera Camera { get; set; } = null!;

    public virtual Employee? Employee { get; set; }
}
