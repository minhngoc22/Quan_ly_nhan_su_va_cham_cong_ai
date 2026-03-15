namespace QLNS.Models.ViewModels
{
    public class ReportViewModel
    {
        // ================= FILTER =================
        public int? DepartmentId { get; set; }
        public DateTime? Month { get; set; }

        // ================= 1️⃣ TỔNG QUAN =================
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public string? LargestDepartment { get; set; }

        // ================= 2️⃣ THEO PHÒNG BAN =================
        public List<DepartmentReportRow> Departments { get; set; } = new();

        // ================= 3️⃣ CHUYÊN CẦN =================
        public int TotalWorkingDays { get; set; }
        public int TotalLateCount { get; set; }

        // ================= TABLE CHÍNH (DÙNG CHUNG) =================
        public List<EmployeeReportRow> Employees { get; set; } = new();

        // ================= 4️⃣ BIẾN ĐỘNG NHÂN SỰ =================
        public int NewEmployees { get; set; }
        public int ResignedEmployees { get; set; }
        public List<EmployeeMovementRow> Movements { get; set; } = new();

        // ================= 5️⃣ CẢNH BÁO / TOP =================
        public List<EmployeeWarningRow> LateTop { get; set; } = new();
        public List<EmployeeWarningRow> AbsentTop { get; set; } = new();

        public decimal TotalWorkUnits { get; set; }

        // ================= 6️⃣ XẾP HẠNG CHUYÊN CẦN =================
        public List<AttendanceRankingRow> AttendanceRanking { get; set; } = new();


    }

    // ======================================================
    // TABLE CHÍNH – DANH SÁCH NHÂN VIÊN
    // ======================================================
    public class EmployeeReportRow
    {
        public string EmployeeCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string Position { get; set; } = "";
        public string Status { get; set; } = "";

        // HR CORE
        public int WorkingDays { get; set; }     // ngày có đi làm
        public int FullDays { get; set; }        // đủ 2 ca
        public int HalfDays { get; set; }        // thiếu ca
        public decimal TotalWorkUnits { get; set; } // CÔNG
        public int LateCount { get; set; }
    }

    // ======================================================
    // PHÒNG BAN
    // ======================================================
    public class DepartmentReportRow
    {
        public string DepartmentName { get; set; } = "";
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
    }

    // ======================================================
    // BIẾN ĐỘNG NHÂN SỰ
    // ======================================================
    public class EmployeeMovementRow
    {
        public string EmployeeCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        public DateTime Date { get; set; }
        public string MovementType { get; set; } = ""; // Join | Resign
    }

    // ======================================================
    // CẢNH BÁO / TOP
    // ======================================================
    public class EmployeeWarningRow
    {
        public string EmployeeCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        public int Count { get; set; }
    }


    // ======================================================
    // XẾP HẠNG CHUYÊN CẦN
    public class AttendanceRankingRow
    {
        public string EmployeeCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        public int EarlyCount { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }

        public int TotalLateMinutes { get; set; }


        // ⭐ THÊM
        public TimeOnly? EarliestCheckIn { get; set; }
    }

}
