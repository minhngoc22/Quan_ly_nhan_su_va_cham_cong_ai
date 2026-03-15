using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public DateTime? HireDate { get; set; }

    public string? Address { get; set; }

    public string? Status { get; set; }

    public string? Avatar { get; set; }

    public int? ExperienceLevelId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<BodyEmbedding> BodyEmbeddings { get; set; } = new List<BodyEmbedding>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<DutyAttendance> DutyAttendances { get; set; } = new List<DutyAttendance>();

    public virtual ICollection<DutySchedule> DutySchedules { get; set; } = new List<DutySchedule>();

    public virtual ICollection<EarlyCheckinLog> EarlyCheckinLogs { get; set; } = new List<EarlyCheckinLog>();

    public virtual ExperienceLevel? ExperienceLevel { get; set; }

    public virtual ICollection<FaceEmbedding> FaceEmbeddings { get; set; } = new List<FaceEmbedding>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<LivenessLog> LivenessLogs { get; set; } = new List<LivenessLog>();

    public virtual ICollection<MovementLog> MovementLogs { get; set; } = new List<MovementLog>();

    public virtual Position? Position { get; set; }

    public virtual ICollection<SecurityAlert> SecurityAlerts { get; set; } = new List<SecurityAlert>();

    public virtual User? User { get; set; }

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
