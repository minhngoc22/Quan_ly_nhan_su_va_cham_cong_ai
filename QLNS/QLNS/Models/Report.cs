using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Report
{
    public int Id { get; set; }

    public string? ReportType { get; set; }

    public DateTime? ReportDate { get; set; }

    public DateTime? CreatedAt { get; set; }
}
