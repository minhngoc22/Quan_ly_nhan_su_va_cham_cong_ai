using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Department
{
    public int Id { get; set; }

    public string? DepartmentCode { get; set; }

    public string? DepartmentName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
