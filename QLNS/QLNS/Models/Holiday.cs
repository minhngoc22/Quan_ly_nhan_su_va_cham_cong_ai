using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Holiday
{
    public int Id { get; set; }

    public DateOnly HolidayDate { get; set; }

    public string? HolidayName { get; set; }

    public bool? IsPaid { get; set; }

    public DateTime? CreatedAt { get; set; }
}
