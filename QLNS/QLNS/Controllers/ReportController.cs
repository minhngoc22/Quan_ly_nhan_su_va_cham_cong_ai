using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Models.ViewModels;
using ClosedXML.Excel;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;


namespace QLNS.Controllers
{
    [Authorize(AuthenticationSchemes = "FaceIDAuth")]
    public class ReportController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public ReportController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        // =============================
        // DASHBOARD BÁO CÁO
        // =============================
        public async Task<IActionResult> Index(int? departmentId, DateTime? month)
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();

            var model = await BuildReport(departmentId, month);

            return View(model);
        }

        // =============================
        // CORE LOGIC – DÙNG CHUNG
        // =============================
        private async Task<ReportViewModel> BuildReport(int? departmentId, DateTime? month)
        {
            // ✅ Nếu không chọn tháng → lấy tháng hiện tại
            month ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var model = new ReportViewModel
            {
                DepartmentId = departmentId,
                Month = month
            };


            // ================= BASE EMP QUERY =================
            var empQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            if (departmentId.HasValue)
                empQuery = empQuery.Where(e => e.DepartmentId == departmentId);

            // ================= KPI NHÂN SỰ =================
            model.TotalEmployees = await empQuery.CountAsync();
            model.ActiveEmployees = await empQuery.CountAsync(e => e.Status == "Đang làm");
            model.InactiveEmployees = model.TotalEmployees - model.ActiveEmployees;

            model.LargestDepartment = await _context.Employees
                .Include(e => e.Department)
                .GroupBy(e => e.Department.DepartmentName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            // ================= PHÒNG BAN =================
            model.Departments = await empQuery
                .GroupBy(e => e.Department.DepartmentName)
                .Select(g => new DepartmentReportRow
                {
                    DepartmentName = g.Key,
                    TotalEmployees = g.Count(),
                    ActiveEmployees = g.Count(x => x.Status == "Đang làm"),
                    InactiveEmployees = g.Count(x => x.Status != "Đang làm")
                })
                .ToListAsync();

            //// ================= CHẤM CÔNG =================
            //if (!month.HasValue)
            //{
            //    model.Employees = await empQuery
            //        .Select(e => new EmployeeReportRow
            //        {
            //            EmployeeCode = e.EmployeeCode,
            //            FullName = e.FullName,
            //            DepartmentName = e.Department.DepartmentName,
            //            Position = e.Position.PositionName,

            //            WorkingDays = 0,
            //            LateCount = 0,
            //            TotalWorkUnits = 0
            //        })
            //        .ToListAsync();

            //    return model;
            //}

            var fromDate = new DateTime(month.Value.Year, month.Value.Month, 1);
            var toDate = fromDate.AddMonths(1);

            var attendanceQuery =
            from a in _context.Attendances
            join s in _context.Shifts on a.ShiftId equals s.Id
            where a.WorkDate >= fromDate
            && a.WorkDate < toDate
            select new
      {
          a.EmployeeId,
          a.CheckTime,
          a.Status,
          ShiftStart = s.StartTime,
          LateThreshold = s.LateThreshold
      };


            //model.TotalWorkingDays = model.Employees.Sum(e => e.WorkingDays);
            model.TotalLateCount = await attendanceQuery.CountAsync(a => a.Status == "Trễ");
            // ============================
            //============== XẾP HẠNG CHUYÊN CẦN ==============
            // LOAD CHẤM CÔNG RA RAM (ĐÓNG DATAREADER)
            var attendanceData = await attendanceQuery.ToListAsync();

            // LOAD NHÂN VIÊN RA RAM
            var employees = await empQuery
                .Include(e => e.Department)
                .ToListAsync();

            // ============== XẾP HẠNG CHUYÊN CẦN ==============
            model.AttendanceRanking = employees
                .Select(e => new AttendanceRankingRow
                {
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    DepartmentName = e.Department.DepartmentName,

                    EarlyCount = attendanceData.Count(a =>
                        a.EmployeeId == e.Id &&
                        TimeOnly.FromDateTime(a.CheckTime) < a.ShiftStart),

                    OnTimeCount = attendanceData.Count(a =>
                        a.EmployeeId == e.Id &&
                        TimeOnly.FromDateTime(a.CheckTime) >= a.ShiftStart &&
                        TimeOnly.FromDateTime(a.CheckTime) <= a.LateThreshold),

                    LateCount = attendanceData.Count(a =>
                        a.EmployeeId == e.Id &&
                        TimeOnly.FromDateTime(a.CheckTime) > a.LateThreshold),

                    TotalLateMinutes = attendanceData
                        .Where(a =>
                            a.EmployeeId == e.Id &&
                            a.LateThreshold != null &&
                            TimeOnly.FromDateTime(a.CheckTime) > a.LateThreshold)
                        .Select(a =>
                            (a.CheckTime.Hour * 60 + a.CheckTime.Minute)
                            -
                            (a.LateThreshold!.Hour * 60 + a.LateThreshold.Minute)
                        )
                        .DefaultIfEmpty(0)
                        .Sum()
                })
                .OrderBy(x => x.TotalLateMinutes)
                .ThenByDescending(x => x.OnTimeCount)
                .ToList();



            // ================= CÔNG THEO NHÂN VIÊN =================
            model.Employees = await empQuery
     .OrderBy(e => e.Department.DepartmentName)
     .ThenBy(e => e.FullName)
     .Select(e => new EmployeeReportRow
     {
         EmployeeCode = e.EmployeeCode,
         FullName = e.FullName,
         DepartmentName = e.Department.DepartmentName,
         Position = e.Position.PositionName,

         WorkingDays = attendanceQuery.Count(a => a.EmployeeId == e.Id),
         LateCount = attendanceQuery.Count(a => a.EmployeeId == e.Id && a.Status == "Trễ"),
         TotalWorkUnits = attendanceQuery
             .Where(a => a.EmployeeId == e.Id)
             .Sum(a =>
                 a.Status == "Đúng giờ" ? 1m :
                 a.Status == "Trễ" ? 0.8m : 0m)
     })
     .ToListAsync();

            // ✅ ĐẶT SAU
            model.TotalWorkingDays = model.Employees.Sum(e => e.WorkingDays);
            model.TotalWorkUnits = model.Employees.Sum(e => e.TotalWorkUnits);


            // ================= TOP TRỄ =================
            model.LateTop = model.Employees
                .OrderByDescending(e => e.LateCount)
                .Take(10)
                .Select(e => new EmployeeWarningRow
                {
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    DepartmentName = e.DepartmentName,
                    Count = e.LateCount
                })
                .ToList();

            // ================= TOP VẮNG MẶT =================
           


            return model;
        }

        // =============================
        // EXPORT EXCEL (stub)
        // =============================
        public async Task<IActionResult> ExportExcel(int? departmentId, DateTime? month)
        {
            // 👉 Nếu không chọn tháng → lấy tháng hiện tại
            if (!month.HasValue)
            {
                var now = DateTime.Now;
                month = new DateTime(now.Year, now.Month, 1);
            }

            var report = await BuildReport(departmentId, month);

            using var wb = new XLWorkbook();

            var ws = wb.Worksheets.Add("Tổng quan");

            // ================= TITLE =================
            ws.Cell("A1").Value = "BÁO CÁO NHÂN SỰ & CHẤM CÔNG";
            ws.Range("A1:J1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ================= [ THÔNG TIN ] =================
            ws.Cell("A3").Value = "THÔNG TIN";
            ws.Cell("A3").Style.Font.SetBold();

            ws.Cell("A4").Value = "Tháng:";
            ws.Cell("B4").Value = report.Month.HasValue
                ? report.Month.Value.ToString("MM/yyyy")
                : "Toàn bộ";

            ws.Cell("A5").Value = "Phòng ban:";
            ws.Cell("B5").Value = "Toàn công ty";

            ws.Cell("A6").Value = "Ngày xuất:";
            ws.Cell("B6").Value = DateTime.Now.ToString("dd/MM/yyyy");

            // ================= [ TỔNG QUAN ] =================
            ws.Cell("E3").Value = "TỔNG QUAN";
            ws.Cell("E3").Style.Font.SetBold();

            ws.Cell("E4").Value = "Tổng NV:";
            ws.Cell("F4").Value = report.TotalEmployees;

            ws.Cell("G4").Value = "Đang làm:";
            ws.Cell("H4").Value = report.ActiveEmployees;

            ws.Cell("I4").Value = "Đã nghỉ:";
            ws.Cell("J4").Value = report.InactiveEmployees;

            ws.Cell("E5").Value = "Tổng công:";
            ws.Cell("F5").Value = report.TotalWorkUnits;

            decimal avgWork = report.TotalEmployees > 0
                ? Math.Round(report.TotalWorkUnits / report.TotalEmployees, 1)
                : 0;

            ws.Cell("G5").Value = "TB công / NV:";
            ws.Cell("H5").Value = avgWork;

            double latePercent = report.TotalEmployees > 0
                ? Math.Round(report.TotalLateCount * 100.0 / report.TotalEmployees, 1)
                : 0;

            ws.Cell("I5").Value = "Lượt đi trễ:";
            ws.Cell("J5").Value = $"{report.TotalLateCount} ({latePercent}%)";

            // ================= [ TOP CÔNG CAO NHẤT ] =================
           
            ws.Cell("A8").Value = "TOP CÔNG CAO NHẤT";
            ws.Cell("A8").Style.Font.SetBold();

            // Header
            ws.Cell("A9").Value = "Mã NV";
            ws.Cell("B9").Value = "Họ tên";
            ws.Cell("C9").Value = "Phòng";
            ws.Cell("D9").Value = "Công";

            ws.Range("A9:D9").Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            int row = 10;
            foreach (var emp in report.Employees
                .OrderByDescending(x => x.TotalWorkUnits)
                .Take(3))
            {
                ws.Cell(row, 1).Value = emp.EmployeeCode;
                ws.Cell(row, 2).Value = emp.FullName;
                ws.Cell(row, 3).Value = emp.DepartmentName;   // ✅ đúng cột Phòng
                ws.Cell(row, 4).Value = emp.TotalWorkUnits;   // ✅ đúng cột Công
                row++;
            }

            // Border data
            ws.Range($"A10:D{row - 1}")
              .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
              .Border.SetInsideBorder(XLBorderStyleValues.Thin);


            // ================= [ TOP ĐI TRỄ ] =================
         
            ws.Cell("F8").Value = "TOP ĐI TRỄ";
            ws.Cell("F8").Style.Font.SetBold();

            // Header
            ws.Cell("F9").Value = "Mã NV";
            ws.Cell("G9").Value = "Họ tên";
            ws.Cell("H9").Value = "Phòng";
            ws.Cell("I9").Value = "Số lần trễ";

            ws.Range("F9:I9").Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            int lateRow = 10;

            // ⚠️ Lọc nhân viên có đi trễ > 0
            foreach (var emp in report.Employees
                .Where(x => x.LateCount > 0)
                .OrderByDescending(x => x.LateCount)
                .Take(3))
            {
                ws.Cell(lateRow, 6).Value = emp.EmployeeCode;
                ws.Cell(lateRow, 7).Value = emp.FullName;
                ws.Cell(lateRow, 8).Value = emp.DepartmentName;
                ws.Cell(lateRow, 9).Value = emp.LateCount;
                lateRow++;
            }

            // Border data
            if (lateRow > 10)
            {
                ws.Range($"F10:I{lateRow - 1}")
                  .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                  .Border.SetInsideBorder(XLBorderStyleValues.Thin);
            }


            // ================= [ TOP ĐI SỚM ] =================
            ws.Cell("K8").Value = "TOP ĐI SỚM";
            ws.Cell("K8").Style.Font.SetBold();

            // Header
            ws.Cell("K9").Value = "Mã NV";
            ws.Cell("L9").Value = "Họ tên";
            ws.Cell("M9").Value = "Phòng";
            ws.Cell("N9").Value = "Số lần sớm";

            ws.Range("K9:N9").Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            int earlyRow = 10;

            foreach (var emp in report.AttendanceRanking
                .Where(x => x.EarlyCount > 0)
                .OrderByDescending(x => x.EarlyCount)
                .Take(3))
            {
                ws.Cell(earlyRow, 11).Value = emp.EmployeeCode;
                ws.Cell(earlyRow, 12).Value = emp.FullName;
                ws.Cell(earlyRow, 13).Value = emp.DepartmentName;
                ws.Cell(earlyRow, 14).Value = emp.EarlyCount;
                earlyRow++;
            }

            // Border data
            if (earlyRow > 10)
            {
                ws.Range($"K10:N{earlyRow - 1}")
                  .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                  .Border.SetInsideBorder(XLBorderStyleValues.Thin);
            }


            // ================= SHEET 2: CÔNG NHÂN VIÊN =================
            var wsEmp = wb.Worksheets.Add("Cong nhan vien");

            // ===== TIÊU ĐỀ =====
            wsEmp.Cell("A1").Value = "BÁO CÁO CÔNG NHÂN VIÊN";
            wsEmp.Range("A1:G1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== THÔNG TIN PHỤ =====
            wsEmp.Cell("A2").Value = "Tháng:";
            wsEmp.Cell("B2").Value = report.Month.HasValue
                ? report.Month.Value.ToString("MM/yyyy")
                : "Toàn bộ thời gian";

            wsEmp.Cell("D2").Value = "Phòng ban:";
            wsEmp.Cell("E2").Value = report.DepartmentId.HasValue
                ? report.Departments.FirstOrDefault()?.DepartmentName ?? "Nhiều phòng"
                : "Toàn công ty";

            wsEmp.Cell("A3").Value = "Ngày xuất:";
            wsEmp.Cell("B3").Value = DateTime.Now.ToString("dd/MM/yyyy");

            // ✅ FIX: style bold đúng chuẩn ClosedXML
            wsEmp.Range("A2:A3").Style.Font.SetBold();
            wsEmp.Cell("D2").Style.Font.SetBold();

            // ===== HEADER BẢNG =====
            var empHeaders = new[]
            {
    "Mã NV", "Họ tên", "Phòng ban", "Chức vụ",
    "Ngày làm", "Đi trễ", "Công"
};

            for (int i = 0; i < empHeaders.Length; i++)
            {
                wsEmp.Cell(5, i + 1).Value = empHeaders[i];
                wsEmp.Cell(5, i + 1).Style
                    .Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            // ===== DỮ LIỆU =====
            int empRow = 6;
            foreach (var e in report.Employees
                .OrderBy(e => e.DepartmentName)
                .ThenBy(e => e.FullName))
            {
                wsEmp.Cell(empRow, 1).Value = e.EmployeeCode;
                wsEmp.Cell(empRow, 2).Value = e.FullName;
                wsEmp.Cell(empRow, 3).Value = e.DepartmentName;
                wsEmp.Cell(empRow, 4).Value = e.Position;
                wsEmp.Cell(empRow, 5).Value = e.WorkingDays;
                wsEmp.Cell(empRow, 6).Value = e.LateCount;
                wsEmp.Cell(empRow, 7).Value = e.TotalWorkUnits;

                empRow++;
            }

            // Border table
            wsEmp.Range($"A5:G{empRow - 1}")
                .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

            // ===== FORMAT =====
            wsEmp.Columns().AdjustToContents();
            wsEmp.SheetView.FreezeRows(5);



            /// ================= SHEET 3: PHÒNG BAN =================
            var wsDept = wb.Worksheets.Add("Phong ban");

            // ===== TIÊU ĐỀ =====
            wsDept.Cell("A1").Value = "BÁO CÁO PHÒNG BAN";
            wsDept.Range("A1:D1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== THÔNG TIN PHỤ =====
            wsDept.Cell("A2").Value = "Tháng:";
            wsDept.Cell("B2").Value = report.Month.HasValue
                ? report.Month.Value.ToString("MM/yyyy")
                : "Toàn bộ thời gian";

            wsDept.Cell("A3").Value = "Ngày xuất:";
            wsDept.Cell("B3").Value = DateTime.Now.ToString("dd/MM/yyyy");

            // Style info
            wsDept.Range("A2:A3").Style.Font.SetBold();

            // ===== HEADER BẢNG =====
            var deptHeaders = new[]
            {
    "Phòng ban", "Tổng NV", "Đang làm", "Đã nghỉ"
};

            for (int i = 0; i < deptHeaders.Length; i++)
            {
                wsDept.Cell(5, i + 1).Value = deptHeaders[i];
                wsDept.Cell(5, i + 1).Style
                    .Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            // ===== DỮ LIỆU =====
            int deptRow = 6;
            foreach (var d in report.Departments)
            {
                wsDept.Cell(deptRow, 1).Value = d.DepartmentName;
                wsDept.Cell(deptRow, 2).Value = d.TotalEmployees;
                wsDept.Cell(deptRow, 3).Value = d.ActiveEmployees;
                wsDept.Cell(deptRow, 4).Value = d.InactiveEmployees;

                deptRow++;
            }

            // Border table
            wsDept.Range($"A5:D{deptRow - 1}")
                .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

            // ===== FORMAT =====
            wsDept.Columns().AdjustToContents();
            wsDept.SheetView.FreezeRows(5); // cố định header giống Sheet 2


            // ================= SHEET 4: CHUYÊN CẦN =================
            var wsAttend = wb.Worksheets.Add("Chuyen can");

            // ===== TIÊU ĐỀ =====
            wsAttend.Cell("A1").Value = "BÁO CÁO CHUYÊN CẦN";
            wsAttend.Range("A1:G1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== THÔNG TIN =====
            wsAttend.Cell("A2").Value = "Tháng:";
            wsAttend.Cell("B2").Value = report.Month.HasValue
                ? report.Month.Value.ToString("MM/yyyy")
                : "Toàn bộ";

            wsAttend.Cell("D2").Value = "Ngày xuất:";
            wsAttend.Cell("E2").Value = DateTime.Now.ToString("dd/MM/yyyy");

            wsAttend.Range("A2:D2").Style.Font.SetBold();

            // ===== HEADER =====
            var headers = new[]
            {
    "Mã NV", "Họ tên", "Phòng ban",
    "Đi sớm", "Đúng giờ", "Đi trễ", "Tổng phút trễ"
};

            for (int i = 0; i < headers.Length; i++)
            {
                wsAttend.Cell(4, i + 1).Value = headers[i];
                wsAttend.Cell(4, i + 1).Style
                    .Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            // ===== DATA =====
            int r = 5;
            foreach (var a in report.AttendanceRanking)
            {
                wsAttend.Cell(r, 1).Value = a.EmployeeCode;
                wsAttend.Cell(r, 2).Value = a.FullName;
                wsAttend.Cell(r, 3).Value = a.DepartmentName;
                wsAttend.Cell(r, 4).Value = a.EarlyCount;
                wsAttend.Cell(r, 5).Value = a.OnTimeCount;
                wsAttend.Cell(r, 6).Value = a.LateCount;
                wsAttend.Cell(r, 7).Value = a.TotalLateMinutes;
                r++;
            }

            // ===== BORDER =====
            wsAttend.Range($"A4:G{r - 1}")
                .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

            // ===== FORMAT =====
            wsAttend.Columns().AdjustToContents();
            wsAttend.SheetView.FreezeRows(4);


            // ================= EXPORT FILE =================
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"BaoCaoNhanSu_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

    }
}
