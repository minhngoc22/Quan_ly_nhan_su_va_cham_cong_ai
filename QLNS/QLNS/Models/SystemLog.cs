using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class SystemLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
