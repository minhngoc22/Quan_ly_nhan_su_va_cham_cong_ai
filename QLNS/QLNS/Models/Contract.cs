using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Contract
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string? ContractType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? BasicSalary { get; set; }

    public decimal? Allowance { get; set; }

    public double? InsuranceRate { get; set; }

    public string? Status { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
