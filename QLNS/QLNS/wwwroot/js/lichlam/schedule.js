var Schedule = (function () {

    /* ================= TẠO CA LÀM ================= */
    function openGenerate() {

        let deptOptions = `<option value="">Tất cả phòng ban</option>`;

        if (window.DEPARTMENTS?.length) {
            window.DEPARTMENTS.forEach(d => {
                deptOptions += `<option value="${d.Id}">${d.DepartmentName}</option>`;
            });
        }

        Swal.fire({
            title: '📅 Tạo lịch làm việc',
            html: buildStyle() + `
                <div class="swal-form">
                    <label>📆 Tháng</label>
                    <input type="month" id="gen-month" class="swal2-input" />

                    <label>🏢 Phòng ban</label>
                    <select id="gen-department" class="swal2-input">
                        ${deptOptions}
                    </select>

                    <label>⏰ Ca làm</label>
                    <select id="gen-shift" class="swal2-input">
                        <option value="">Cả ngày</option>
                        <option value="Morning">Sáng</option>
                        <option value="Afternoon">Chiều</option>
                    </select>
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Tạo lịch',
            preConfirm: () => {
                const month = document.getElementById('gen-month').value;
                if (!month) {
                    Swal.showValidationMessage('⚠️ Vui lòng chọn tháng');
                    return false;
                }

                return {
                    month,
                    departmentId: document.getElementById('gen-department').value || null,
                    shiftType: document.getElementById('gen-shift').value
                };
            }
        }).then(r => {
            if (!r.isConfirmed) return;

            fetch('/Schedule/GenerateMonth', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(res =>
                    Swal.fire('✅ Thành công', res.message, 'success')
                        .then(() => location.reload())
                );
        });
    }

    /* ================= TẠO LỊCH TRỰC ================= */
    function openGenerateDuty() {

        Swal.fire({
            title: '🛎️ Tạo lịch trực',
            html: buildStyle() + `
                <div class="swal-form">
                    <label>🏢 Phòng ban</label>
                    <select id="duty-dept" class="swal2-input">
                        ${window.DEPARTMENTS.map(d =>
                `<option value="${d.Id}">${d.DepartmentName}</option>`
            ).join('')}
                    </select>

                    <label>📅 Từ ngày</label>
                    <input type="date" id="fromDate" class="swal2-input">

                    <label>📅 Đến ngày</label>
                    <input type="date" id="toDate" class="swal2-input">

                    <label>⚙️ Quy tắc</label>
                    <select id="rule" class="swal2-input">
                        <option value="ROTATE">Luân phiên</option>
                        <option value="FIXED">Cố định</option>
                    </select>
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Xem trước',
            preConfirm: () => {
                const from = document.getElementById('fromDate').value;
                const to = document.getElementById('toDate').value;

                if (!from || !to || from > to) {
                    Swal.showValidationMessage('⚠️ Ngày không hợp lệ');
                    return false;
                }

                return {
                    departmentId: +document.getElementById('duty-dept').value,
                    fromDate: from,
                    toDate: to,
                    rule: document.getElementById('rule').value
                };
            }
        }).then(r => {
            if (!r.isConfirmed) return;

            fetch('/Schedule/PreviewDuty', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(showPreviewDuty);
        });
    }

    /* ================= PREVIEW ================= */
    function showPreviewDuty(data) {

        let rows = data.map(x => `
<tr>
    <td>${x.employeeName}</td>
    <td>${new Date(x.dutyDate).toLocaleDateString()}</td>
    <td>${x.dutyShiftName}</td>
</tr>
`).join('');


        Swal.fire({
            title: '👀 Xem trước lịch trực',
            html: `
        <table class="table table-sm">
            <thead>
                <tr>
                    <th>Nhân viên</th>
                    <th>Ngày</th>
                    <th>Ca trực</th>
                </tr>
            </thead>
            <tbody>${rows}</tbody>
        </table>
    `,
            confirmButtonText: '✔ Tạo lịch',
            showCancelButton: true
        }).then(r => {
            if (!r.isConfirmed) return;

            fetch('/Schedule/GenerateDutySchedule', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(r => r.json()) // 👈 bắt response
                .then(res => {
                    Swal.fire({
                        icon: 'success',
                        title: '✅ Tạo lịch trực thành công',
                        text: res?.message || 'Lịch trực đã được lưu vào hệ thống',
                        confirmButtonText: 'OK'
                    }).then(() => location.reload());
                })
                .catch(() => {
                    Swal.fire('❌ Lỗi', 'Không thể tạo lịch trực', 'error');
                });
        });
    }
    function openLeave(scheduleId) {

        Swal.fire({
            title: '📝 Tạo nghỉ phép',
            html: buildStyle() + `
        <div class="swal-form">

            <label>📅 Từ ngày</label>
            <input type="date" id="from-date" class="swal2-input"/>

            <label>📅 Đến ngày</label>
            <input type="date" id="to-date" class="swal2-input"/>

            <label>⏰ Loại thời gian</label>
            <select id="leave-session" class="swal2-input">
                <option value="FULL">Cả ngày</option>
                <option value="MORNING">Nửa ngày sáng</option>
                <option value="AFTERNOON">Nửa ngày chiều</option>
            </select>

            <label>📄 Loại nghỉ</label>
            <select id="leave-type" class="swal2-input">
                <option value="Phép năm">Phép năm</option>
                <option value="Ốm">Ốm</option>
          
                <option value="Không lương">Không lương</option>
            </select>

        </div>
        `,
            showCancelButton: true,
            confirmButtonText: 'Lưu',

            preConfirm: () => {

                const fromDate = document.getElementById('from-date').value;
                const toDate = document.getElementById('to-date').value;
                const leaveType = document.getElementById('leave-type').value;
                const session = document.getElementById('leave-session').value;

                if (!fromDate || !toDate) {
                    Swal.showValidationMessage('⚠️ Chọn đầy đủ ngày');
                    return false;
                }

                return {
                    scheduleId: scheduleId,
                    fromDate: fromDate,
                    toDate: toDate,
                    session: session,
                    leaveType: leaveType
                };
            }
        }).then(r => {

            if (!r.isConfirmed) return;

            fetch('/Leave/CreateCompanyLeave', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(res => {

                    if (res.success) {

                        Swal.fire({
                            icon: 'success',
                            title: '✅ Thành công',
                            text: `Đã tạo nghỉ cho ${res.count ?? 0} nhân viên`
                        }).then(() => location.reload());

                    } else {

                        Swal.fire({
                            icon: 'info',
                            title: 'Thông báo',
                            text: res.message || 'Không có thay đổi'
                        });

                    }

                })
                .catch(err => {

                    console.error(err);

                    Swal.fire({
                        icon: 'error',
                        title: '❌ Lỗi',
                        text: 'Không thể tạo lịch nghỉ'
                    });

                });
        });
    }

    /* ================= NGHỈ TOÀN CÔNG TY ================= */
    function openLeaveAll() {

        Swal.fire({
            title: '🏖️ Nghỉ toàn công ty',
            html: buildStyle() + `
        <div class="swal-form">

            <label>Từ ngày</label>
            <input type="date" id="leave-from" class="swal2-input"/>

            <label>Đến ngày</label>
            <input type="date" id="leave-to" class="swal2-input"/>

            <label>Loại nghỉ</label>
            <select id="leave-type" class="swal2-input">
                <option value="Lễ">Lễ</option>
                <option value="Tết">Tết</option>
                <option value="Nghỉ công ty">Nghỉ công ty</option>
            </select>

        </div>
        `,
            showCancelButton: true,
            confirmButtonText: 'Tạo',
            preConfirm: () => {

                const from = document.getElementById('leave-from').value;
                const to = document.getElementById('leave-to').value;

                if (!from || !to) {
                    Swal.showValidationMessage('Chọn đầy đủ ngày');
                    return false;
                }

                if (from > to) {
                    Swal.showValidationMessage('Ngày không hợp lệ');
                    return false;
                }

                return {
                    fromDate: from,
                    toDate: to,
                    leaveType: document.getElementById('leave-type').value
                };
            }
        }).then(result => {

            if (!result.isConfirmed) return;

            fetch('/Leave/CreateCompanyLeave', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(result.value)
            })
                .then(async r => {

                    const text = await r.text();
                    console.log("SERVER RESPONSE:", text);

                    try {
                        return JSON.parse(text);
                    } catch (e) {
                        throw new Error("Response không phải JSON");
                    }

                })
                .then(res => {

                    if (res.success) {

                        Swal.fire({
                            icon: 'success',
                            title: '✅ Thành công',
                            text: `Đã tạo nghỉ cho ${res.count ?? 0} nhân viên`
                        }).then(() => location.reload());

                    } else {

                        Swal.fire({
                            icon: 'info',
                            title: 'Thông báo',
                            text: res.message || 'Không có thay đổi'
                        });

                    }

                })
                .catch(err => {

                    console.error("API ERROR:", err);

                    Swal.fire({
                        icon: 'error',
                        title: '❌ Lỗi',
                        text: 'Không thể tạo lịch nghỉ'
                    });

                });
        });

    }


    function deleteSchedule(id, type) {

        let url = '';

        if (type === 'WORK')
            url = `/Schedule/DeleteWork?id=${id}`;

        if (type === 'DUTY')
            url = `/Schedule/DeleteDuty?id=${id}`;

        if (type === 'LEAVE')
            url = `/Schedule/DeleteLeave?id=${id}`;

        Swal.fire({
            title: 'Xóa lịch?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa'
        }).then(result => {

            if (!result.isConfirmed) return;

            fetch(url, { method: 'DELETE' })
                .then(r => r.json())
                .then(res => {

                    if (res.success) {

                        Swal.fire({
                            icon: 'success',
                            title: 'Đã xóa',
                            timer: 1200,
                            showConfirmButton: false
                        });

                        setTimeout(() => location.reload(), 1200);
                    }
                    else {
                        Swal.fire('Lỗi', res.message, 'error');
                    }

                })
                .catch(() => {
                    Swal.fire('Lỗi', 'Không thể xóa', 'error');
                });

        });
    }

    function openLeaveEmployee(scheduleId) {

        Swal.fire({
            title: '📝 Sửa lịch làm (tạo nghỉ)',
            html: buildStyle() + `
        <div class="swal-form">

            <label>📅 Từ ngày</label>
            <input type="date" id="from-date" class="swal2-input"/>

            <label>📅 Đến ngày</label>
            <input type="date" id="to-date" class="swal2-input"/>

            <label>⏰ Loại thời gian</label>
            <select id="leave-session" class="swal2-input">
                <option value="FULL">Cả ngày</option>
                <option value="MORNING">Nửa ngày sáng</option>
                <option value="AFTERNOON">Nửa ngày chiều</option>
            </select>

            <label>📄 Loại nghỉ</label>
            <select id="leave-type" class="swal2-input">
                <option value="Phép năm">Phép năm</option>
                <option value="Ốm">Ốm</option>
                <option value="Không lương">Không lương</option>
            </select>

        </div>
        `,
            showCancelButton: true,
            confirmButtonText: 'Lưu',

            preConfirm: () => {

                const fromDate = document.getElementById('from-date').value;
                const toDate = document.getElementById('to-date').value;
                const leaveType = document.getElementById('leave-type').value;
                const session = document.getElementById('leave-session').value;

                if (!fromDate || !toDate) {
                    Swal.showValidationMessage('⚠️ Chọn đầy đủ ngày');
                    return false;
                }

                return {
                    scheduleId: scheduleId,
                    fromDate: fromDate,
                    toDate: toDate,
                    session: session,
                    leaveType: leaveType
                };
            }
        }).then(r => {

            if (!r.isConfirmed) return;

            fetch('/Leave/CreateFromSchedule', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(res => {

                    if (res.success) {

                        Swal.fire({
                            icon: 'success',
                            title: '✅ Thành công',
                            text: 'Đã cập nhật lịch nghỉ của nhân viên'
                        }).then(() => location.reload());

                    } else {

                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi',
                            text: res.message || 'Không thể tạo nghỉ'
                        });

                    }

                })
                .catch(() => {

                    Swal.fire({
                        icon: 'error',
                        title: '❌ Lỗi',
                        text: 'Không thể kết nối server'
                    });

                });

        });
    }
    /* ================= STYLE DÙNG CHUNG ================= */
    function buildStyle() {
        return `
            <style>
                .swal-form {
                    display: grid;
                    grid-template-columns: 110px 1fr;
                    gap: 12px 14px;
                    align-items: center;
                    margin-top: 10px;
                }
                .swal-form label {
                    font-weight: 500;
                    color: #555;
                }
                .swal-form .swal2-input,
                .swal-form select {
                    width: 100%;
                    margin: 0 !important;
                }
            </style>
        `;
    }

    /* ================= EXPORT ================= */
    return {
        openGenerate,
        openGenerateDuty,
        openLeave,
        openLeaveAll,
        openLeaveEmployee,
        deleteSchedule
    };

})();
