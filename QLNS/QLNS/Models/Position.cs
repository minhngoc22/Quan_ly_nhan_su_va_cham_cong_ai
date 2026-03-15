using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Position
{
    public int Id { get; set; }

    public string? PositionCode { get; set; }

    public string? PositionName { get; set; }

    public decimal? BaseSalary { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
