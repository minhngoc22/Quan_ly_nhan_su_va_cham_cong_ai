using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class EarlyCheckinLog
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int ShiftId { get; set; }

    public DateTime DetectedTime { get; set; }

    public int? CameraId { get; set; }

    public double? SimilarityScore { get; set; }

    public bool? IsConverted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateOnly WorkDate { get; set; }

    public virtual Camera? Camera { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;
}
