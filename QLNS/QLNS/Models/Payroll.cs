using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Payroll
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int ContractId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int? WorkingDays { get; set; }

    public int? LateCount { get; set; }

    public decimal? BasicSalary { get; set; }

    public decimal? Allowance { get; set; }

    public decimal? Deduction { get; set; }

    public decimal? TotalSalary { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
