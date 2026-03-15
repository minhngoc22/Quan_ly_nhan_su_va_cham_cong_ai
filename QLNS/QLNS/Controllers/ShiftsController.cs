using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Models.ViewModels;
using System;

namespace QLNS.Controllers
{
    public class ShiftsController : Controller
    {
        private readonly FaceIdHrmsContext _context;

        public ShiftsController(FaceIdHrmsContext context)
        {
            _context = context;
        }

        // ================= DANH SÁCH CA =================
        public async Task<IActionResult> Index(string shiftType = "WORK")
        {
            ViewBag.ShiftType = shiftType;

            var now = TimeOnly.FromDateTime(DateTime.Now);

            var result = new List<ShiftVM>();

            if (shiftType == "WORK")
            {
                var shifts = await _context.Shifts
                    .Where(x => x.IsActive)
                    .ToListAsync();

                result = shifts.Select(s => new ShiftVM
                {
                    Id = s.Id,
                    Name = s.ShiftName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    LateThreshold = s.LateThreshold,
                    IsActive = s.IsActive,
                    AllowAttendance = s.AllowAttendance,
                    IsAttendanceOpen =
                        now >= s.StartTime &&
                        now <= s.EndTime &&
                        s.AllowAttendance,
                    Type = "WORK"
                }).ToList();
            }
            else
            {
                var duties = await _context.DutyShifts
                    .Where(x => x.IsActive)
                    .ToListAsync();

                result = duties.Select(d => new ShiftVM
                {
                    Id = d.Id,
                    Name = d.DutyName,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime,
                    IsActive = d.IsActive,
                    AllowAttendance = d.AllowAttendance,
                    IsAttendanceOpen =
                        now >= d.StartTime &&
                        now <= d.EndTime &&
                        d.AllowAttendance,
                    Type = "DUTY"
                }).ToList();
            }

            return View(result);
        }
        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Shift model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.IsActive = true;
            model.IsAttendanceOpen = true;

            _context.Shifts.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
                return NotFound();

            return View(shift);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Shift model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var shift = await _context.Shifts.FindAsync(model.Id);

            if (shift == null)
                return NotFound();

            shift.ShiftName = model.ShiftName;
            shift.StartTime = model.StartTime;
            shift.EndTime = model.EndTime;
            shift.LateThreshold = model.LateThreshold;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= ĐÓNG / MỞ CHẤM CÔNG =================
        [HttpPost]
        public async Task<IActionResult> ToggleAttendance(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
                return Json(new { success = false });

            shift.AllowAttendance = !shift.AllowAttendance;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id, string type)
        {
            if (type == "WORK")
            {
                var shift = await _context.Shifts.FindAsync(id);
                shift.IsActive = !shift.IsActive;
            }
            else
            {
                var duty = await _context.DutyShifts.FindAsync(id);
                duty.IsActive = !duty.IsActive;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ================= XOÁ MỀM =================
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);

            if (shift != null)
            {
                shift.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= CREATE AJAX =================
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] ShiftVM model)
        {
            if (model == null)
                return Json(new { success = false });

            // ===== CA LÀM =====
            if (model.Type == "WORK")
            {
                var shift = new Shift
                {
                    ShiftName = model.Name,
                    StartTime = model.StartTime.Value,
                    EndTime = model.EndTime.Value,
                    LateThreshold = model.LateThreshold,
                    IsActive = true,
                    AllowAttendance = true
                };

                _context.Shifts.Add(shift);
            }
            // ===== CA TRỰC =====
            else if (model.Type == "DUTY")
            {
                var duty = new DutyShift
                {
                    DutyName = model.Name,
                    StartTime = model.StartTime.Value,
                    EndTime = model.EndTime.Value,
                    IsActive = true,
                    AllowAttendance = true
                };

                _context.DutyShifts.Add(duty);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> EditAjax([FromBody] ShiftVM model)
        {
            if (model == null)
                return Json(new { success = false });

            if (model.Type == "WORK")
            {
                var shift = await _context.Shifts.FindAsync(model.Id);

                if (shift == null)
                    return Json(new { success = false });

                if (string.IsNullOrEmpty(model.Name))
                    return Json(new { success = false, message = "Tên ca không hợp lệ" });
                shift.StartTime = model.StartTime.Value;
                shift.StartTime = model.StartTime.Value;
                shift.EndTime = model.EndTime.Value;
                shift.LateThreshold = model.LateThreshold;
                shift.IsActive = model.IsActive;
            }
            else
            {
                var duty = await _context.DutyShifts.FindAsync(model.Id);

                if (duty == null)
                    return Json(new { success = false });

                duty.DutyName = model.Name;
                duty.StartTime = model.StartTime.Value;
                duty.EndTime = model.EndTime.Value;
                duty.IsActive = model.IsActive;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}