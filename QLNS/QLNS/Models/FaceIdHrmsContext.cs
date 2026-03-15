using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLNS.Models;

public partial class FaceIdHrmsContext : DbContext
{
    public FaceIdHrmsContext()
    {
    }

    public FaceIdHrmsContext(DbContextOptions<FaceIdHrmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<BodyEmbedding> BodyEmbeddings { get; set; }

    public virtual DbSet<Camera> Cameras { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DutyAttendance> DutyAttendances { get; set; }

    public virtual DbSet<DutySchedule> DutySchedules { get; set; }

    public virtual DbSet<DutyShift> DutyShifts { get; set; }

    public virtual DbSet<EarlyCheckinLog> EarlyCheckinLogs { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ExperienceLevel> ExperienceLevels { get; set; }

    public virtual DbSet<FaceEmbedding> FaceEmbeddings { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<LivenessLog> LivenessLogs { get; set; }

    public virtual DbSet<MovementLog> MovementLogs { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SecurityAlert> SecurityAlerts { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<WorkSchedule> WorkSchedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-GOC9P0UG;Database=FaceID_HRMS;User Id=sa;Password=123;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07023C8998");

            entity.ToTable("Attendance");

            entity.HasIndex(e => new { e.EmployeeId, e.ShiftId, e.WorkDate }, "UX_Attendance").IsUnique();

            entity.Property(e => e.CheckTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.WorkDate)
                .HasComputedColumnSql("(CONVERT([datetime],[CheckTime]))", true)
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Employees");

            entity.HasOne(d => d.Shift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Shifts");
        });

        modelBuilder.Entity<BodyEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BodyEmbe__3214EC0763B6903B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.BodyEmbeddings)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BodyEmbeddings_Employees");
        });

        modelBuilder.Entity<Camera>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cameras__3214EC077D32C519");

            entity.HasIndex(e => e.CameraCode, "UQ__Cameras__27375FE6D814C439").IsUnique();

            entity.Property(e => e.CameraCode).HasMaxLength(50);
            entity.Property(e => e.CameraName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location).HasMaxLength(255);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07FA1509E7");

            entity.HasIndex(e => e.DepartmentCode, "UQ__Departme__6EA8896DECA82EEA").IsUnique();

            entity.Property(e => e.DepartmentCode).HasMaxLength(10);
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<DutyAttendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DutyAtte__3214EC0788C2B05B");

            entity.ToTable("DutyAttendance");

            entity.HasIndex(e => new { e.EmployeeId, e.DutyShiftId, e.DutyDate }, "UX_DutyAttendance").IsUnique();

            entity.Property(e => e.CheckTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DutyDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.DutyShift).WithMany(p => p.DutyAttendances)
                .HasForeignKey(d => d.DutyShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DutyAttendance_DutyShifts");

            entity.HasOne(d => d.Employee).WithMany(p => p.DutyAttendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DutyAttendance_Employees");
        });

        modelBuilder.Entity<DutySchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DutySche__3214EC07B4EE6CD2");

            entity.Property(e => e.DutyDate).HasColumnType("datetime");

            entity.HasOne(d => d.DutyShift).WithMany(p => p.DutySchedules)
                .HasForeignKey(d => d.DutyShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DutySchedules_DutyShifts");

            entity.HasOne(d => d.Employee).WithMany(p => p.DutySchedules)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DutySchedules_Employees");
        });

        modelBuilder.Entity<DutyShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DutyShif__3214EC0775942927");

            entity.Property(e => e.AllowAttendance).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DutyName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<EarlyCheckinLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EarlyChe__3214EC0776ECFD6A");

            entity.HasIndex(e => e.EmployeeId, "IX_EarlyCheckinLogs_Employee");

            entity.HasIndex(e => e.DetectedTime, "IX_EarlyCheckinLogs_Time").IsDescending();

            entity.HasIndex(e => new { e.EmployeeId, e.ShiftId, e.WorkDate }, "UX_EarlyCheckin").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DetectedTime).HasColumnType("datetime");
            entity.Property(e => e.IsConverted).HasDefaultValue(false);
            entity.Property(e => e.WorkDate).HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.HasOne(d => d.Camera).WithMany(p => p.EarlyCheckinLogs)
                .HasForeignKey(d => d.CameraId)
                .HasConstraintName("FK_EarlyCheckinLogs_Cameras");

            entity.HasOne(d => d.Employee).WithMany(p => p.EarlyCheckinLogs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EarlyCheckinLogs_Employees");

            entity.HasOne(d => d.Shift).WithMany(p => p.EarlyCheckinLogs)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EarlyCheckinLogs_Shifts");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC07E8F0215A");

            entity.HasIndex(e => e.EmployeeCode, "UQ__Employee__1F642548FD1D0AC6").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.HireDate).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đang làm");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Employees_Departments");

            entity.HasOne(d => d.ExperienceLevel).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ExperienceLevelId)
                .HasConstraintName("FK_Employees_ExperienceLevels");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK_Employees_Positions");
        });

        modelBuilder.Entity<ExperienceLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Experien__3214EC07A1AB3EAF");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.LevelName).HasMaxLength(50);
        });

        modelBuilder.Entity<FaceEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FaceEmbe__3214EC07AD642B87");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.FaceEmbeddings)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FaceEmbeddings_Employees");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveReq__3214EC07C50FCE32");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FromDate).HasColumnType("datetime");
            entity.Property(e => e.IsCompanyLeave).HasDefaultValue(false);
            entity.Property(e => e.LeaveType).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ duyệt");
            entity.Property(e => e.ToDate).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequests_Employees");
        });

        modelBuilder.Entity<LivenessLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Liveness__3214EC071FBA3EA9");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.LivenessLogs)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_LivenessLogs_Employees");
        });

        modelBuilder.Entity<MovementLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Movement__3214EC07985D42FA");

            entity.HasIndex(e => e.CreatedAt, "IX_MovementLogs_CreatedAt").IsDescending();

            entity.HasIndex(e => e.PersonType, "IX_MovementLogs_PersonType");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PersonType).HasMaxLength(50);
            entity.Property(e => e.SnapshotPath).HasMaxLength(255);
            entity.Property(e => e.TrackingId).HasMaxLength(100);

            entity.HasOne(d => d.Camera).WithMany(p => p.MovementLogs)
                .HasForeignKey(d => d.CameraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovementLogs_Cameras");

            entity.HasOne(d => d.Employee).WithMany(p => p.MovementLogs)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_MovementLogs_Employees");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Position__3214EC07380808D4");

            entity.HasIndex(e => e.PositionCode, "UQ__Position__83745B02F6557447").IsUnique();

            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PositionCode).HasMaxLength(10);
            entity.Property(e => e.PositionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reports__3214EC07E8624DFA");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReportDate).HasColumnType("datetime");
            entity.Property(e => e.ReportType).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC073CE199BB");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<SecurityAlert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Security__3214EC077AA80678");

            entity.HasIndex(e => e.AlertType, "IX_SecurityAlerts_AlertType");

            entity.HasIndex(e => e.CreatedAt, "IX_SecurityAlerts_CreatedAt").IsDescending();

            entity.Property(e => e.AlertType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsSentToDiscord).HasDefaultValue(false);
            entity.Property(e => e.OccurrenceCount).HasDefaultValue(1);

            entity.HasOne(d => d.Camera).WithMany(p => p.SecurityAlerts)
                .HasForeignKey(d => d.CameraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SecurityAlerts_Cameras");

            entity.HasOne(d => d.Employee).WithMany(p => p.SecurityAlerts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_SecurityAlerts_Employees");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shifts__3214EC0781957725");

            entity.Property(e => e.AllowAttendance).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAttendanceOpen).HasDefaultValue(true);
            entity.Property(e => e.ShiftName).HasMaxLength(50);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemLo__3214EC07C2741262");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.SystemLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_SystemLogs_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0714687686");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4198EC43C").IsUnique();

            entity.HasIndex(e => e.EmployeeId, "UQ__Users__7AD04F10E8A90E4A").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFirstLogin).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Employees");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC0782D01F50");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<WorkSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkSche__3214EC07C7692875");

            entity.Property(e => e.WorkDate).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_WorkSchedules_Employees");

            entity.HasOne(d => d.Shift).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_WorkSchedules_Shifts");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
