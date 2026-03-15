using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Models.ViewModels;
using ClosedXML.Excel;

namespace QLNS.Controllers
{
    [Authorize(AuthenticationSchemes = "FaceIDAuth", Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public AdminReportsController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        // =============================
        // DASHBOARD REPORT ADMIN
        // =============================
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var model = await BuildAdminReport(today);

            return View(model);
        }

        // =============================
        // CORE LOGIC ADMIN
        // =============================
        private async Task<AdminReportVM> BuildAdminReport(DateTime date)
        {
            var model = new AdminReportVM();

            // ===== DATE RANGE (TRÁNH .Date BUG EF) =====
            var start = date.Date;
            var end = start.AddDays(1);

            // ========= EMPLOYEE =========
            model.TotalEmployees =
                await _context.Employees.CountAsync();

            model.ActiveEmployees =
                await _context.Employees
                    .CountAsync(e => e.Status == "Đang làm");

            // ========= USER =========
            model.TotalUsers =
                await _context.Users.CountAsync();

            // ========= CAMERA =========
            model.ActiveCameras =
                await _context.Cameras
                    .CountAsync(c => c.IsActive==true);

            // ========= ATTENDANCE TODAY =========
            var todayAttendance = _context.Attendances
                .Where(a => a.CheckTime >= start && a.CheckTime < end);

            model.TodayAttendance =
                await todayAttendance.CountAsync();

            model.LateToday =
                await todayAttendance
                    .CountAsync(a => a.Status == "Trễ");

            // ========= AI ALERT =========
            var alerts = _context.SecurityAlerts
                .Where(a =>
                    a.CreatedAt.HasValue &&
                    a.CreatedAt >= start &&
                    a.CreatedAt < end);

            model.TodayAlerts =
                await alerts.CountAsync();

            model.LivenessFails =
                await alerts.CountAsync(a => a.AlertType == "LivenessFail");

            model.FaceSpoofing =
                await alerts.CountAsync(a => a.AlertType == "Spoofing");

            // ========= UNKNOWN PERSON =========
            model.UnknownPersons =
                await _context.MovementLogs
                    .CountAsync(m =>
                        m.PersonType == "Unknown" &&
                        m.CreatedAt >= start &&
                        m.CreatedAt < end);

            // ========= TOP ACTIVE USERS =========
            model.TopUsers = await _context.SystemLogs
                .Include(l => l.User)
                .Where(l => l.CreatedAt >= start && l.CreatedAt < end)
                .GroupBy(l => l.User.Username)
                .Select(g => new TopUserRow
                {
                    Username = g.Key,
                    TotalActions = g.Count()
                })
                .OrderByDescending(x => x.TotalActions)
                .Take(5)
                .ToListAsync();

            // ========= ALERT BY TYPE =========
            model.AlertByType = await _context.SecurityAlerts
                .Where(a =>
                    a.CreatedAt.HasValue &&
                    a.CreatedAt >= start &&
                    a.CreatedAt < end)
                .GroupBy(a => a.AlertType)
                .Select(g => new AlertTypeRow
                {
                    AlertType = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            return model;
        }
        // =============================
        // EXPORT ADMIN REPORT
        // =============================
        public async Task<IActionResult> ExportExcel(DateTime? date)
        {
            date ??= DateTime.Today;

            var report = await BuildAdminReport(date.Value);

            using var wb = new XLWorkbook();

            // =====================================================
            // SHEET 1 - OVERVIEW
            // =====================================================
            var ws = wb.Worksheets.Add("Overview");

            ws.Cell("A1").Value = "BÁO CÁO HỆ THỐNG AI HRMS";
            ws.Range("A1:D1").Merge();

            ws.Cell("A1").Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A3").Value = "Ngày báo cáo:";
            ws.Cell("B3").Value = date.Value.ToString("dd/MM/yyyy");

            ws.Cell("A5").Value = "Tổng nhân viên";
            ws.Cell("B5").Value = report.TotalEmployees;

            ws.Cell("A6").Value = "Nhân viên hoạt động";
            ws.Cell("B6").Value = report.ActiveEmployees;

            ws.Cell("A7").Value = "Tổng User";
            ws.Cell("B7").Value = report.TotalUsers;

            ws.Cell("A8").Value = "Camera hoạt động";
            ws.Cell("B8").Value = report.ActiveCameras;

            ws.Cell("A9").Value = "Checkin hôm nay";
            ws.Cell("B9").Value = report.TodayAttendance;

            ws.Cell("A10").Value = "Đi trễ";
            ws.Cell("B10").Value = report.LateToday;

            ws.Cell("A11").Value = "AI Alerts";
            ws.Cell("B11").Value = report.TodayAlerts;

            ws.Cell("A12").Value = "Unknown Persons";
            ws.Cell("B12").Value = report.UnknownPersons;

            ws.Columns().AdjustToContents();

            // =====================================================
            // SHEET 2 - TOP USERS
            // =====================================================
            var wsUsers = wb.Worksheets.Add("Top Users");

            wsUsers.Cell("A1").Value = "TOP USER HOẠT ĐỘNG";
            wsUsers.Range("A1:C1").Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            wsUsers.Cell("A3").Value = "#";
            wsUsers.Cell("B3").Value = "Username";
            wsUsers.Cell("C3").Value = "Total Actions";

            wsUsers.Range("A3:C3").Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray);

            int r = 4;
            int rank = 1;

            foreach (var u in report.TopUsers)
            {
                wsUsers.Cell(r, 1).Value = rank++;
                wsUsers.Cell(r, 2).Value = u.Username;
                wsUsers.Cell(r, 3).Value = u.TotalActions;
                r++;
            }

            wsUsers.Columns().AdjustToContents();

            // =====================================================
            // SHEET 3 - ALERT BY TYPE
            // =====================================================
            var wsAlert = wb.Worksheets.Add("AI Alerts");

            wsAlert.Cell("A1").Value = "THỐNG KÊ AI ALERT";
            wsAlert.Range("A1:B1").Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            wsAlert.Cell("A3").Value = "Alert Type";
            wsAlert.Cell("B3").Value = "Total";

            wsAlert.Range("A3:B3").Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray);

            int ar = 4;
            foreach (var a in report.AlertByType)
            {
                wsAlert.Cell(ar, 1).Value = a.AlertType;
                wsAlert.Cell(ar, 2).Value = a.Total;
                ar++;
            }

            wsAlert.Columns().AdjustToContents();

            // =====================================================
            // SHEET 4 - ATTENDANCE TODAY (DETAIL)
            // =====================================================
            var wsAtt = wb.Worksheets.Add("Attendance Today");

            wsAtt.Cell("A1").Value = "CHI TIẾT CHẤM CÔNG HÔM NAY";
            wsAtt.Range("A1:E1").Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            wsAtt.Cell("A3").Value = "Employee";
            wsAtt.Cell("B3").Value = "Check Time";
            wsAtt.Cell("C3").Value = "Status";
            wsAtt.Cell("D3").Value = "Camera";
            wsAtt.Cell("E3").Value = "Confidence";

            wsAtt.Range("A3:E3").Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray);

            var attendance = await _context.Attendances
                .Include(a => a.Employee)
                
                .Where(a => a.CheckTime.Date == date)
                .OrderByDescending(a => a.CheckTime)
                .ToListAsync();

            int atRow = 4;

            foreach (var a in attendance)
            {
                wsAtt.Cell(atRow, 1).Value = a.Employee?.FullName;
                wsAtt.Cell(atRow, 2).Value = a.CheckTime;
                wsAtt.Cell(atRow, 3).Value = a.Status;
           
                

                atRow++;
            }

            wsAtt.Columns().AdjustToContents();

            // =====================================================
            // EXPORT
            // =====================================================
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AdminReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}