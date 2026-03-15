using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class User
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsFirstLogin { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
