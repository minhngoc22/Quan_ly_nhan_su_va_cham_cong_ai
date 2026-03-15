using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class ExperienceLevel
{
    public int Id { get; set; }

    public string? LevelName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
