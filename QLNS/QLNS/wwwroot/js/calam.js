var Shift = (function () {
    /* ================= TAB SWITCH ================= */
    function switchTab(event, tabId) {

        // Ẩn toàn bộ tab
        document.querySelectorAll(".tab-content")
            .forEach(t => t.classList.remove("active"));

        // bỏ active button
        document.querySelectorAll(".tab-btn")
            .forEach(b => b.classList.remove("active"));

        // hiện tab được chọn
        document.getElementById(tabId)
            .classList.add("active");

        // active button
        event.currentTarget.classList.add("active");
    }
    /* ================= STYLE ================= */
    function style() {
        return `
        <style>
            .swal-form{
                display:grid;
                grid-template-columns:130px 1fr;
                gap:12px;
                align-items:center;
            }
        </style>`;
    }

    /* ================= CREATE ================= */
    function openCreate(type) {

        Swal.fire({
            title: type === "DUTY"
                ? "➕ Thêm ca trực"
                : "➕ Thêm ca làm",
            width: 500,
            html: style() + `
                <div class="swal-form">
                    <label>Tên ca</label>
                    <input id="shiftName" class="swal2-input">

                    <label>Giờ bắt đầu</label>
                    <input type="time" id="startTime" class="swal2-input">

                    <label>Giờ kết thúc</label>
                    <input type="time" id="endTime" class="swal2-input">

                    <label>Mốc trễ</label>
                    <input type="time" id="lateTime" class="swal2-input">
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Lưu',

            preConfirm: () => {

                const name = document.getElementById('shiftName').value;
                const start = document.getElementById('startTime').value;
                const end = document.getElementById('endTime').value;

                if (!name || !start || !end) {
                    Swal.showValidationMessage("Nhập đầy đủ thông tin");
                    return false;
                }

                return {
                    shiftName: name,
                    startTime: start,
                    endTime: end,
                    lateThreshold: document.getElementById('lateTime').value,
                    type: type
                };
            }

        }).then(r => {

            if (!r.isConfirmed) return;

            fetch('/Shifts/CreateAjax', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(res => {

                    if (res.success)
                        Swal.fire({
                            icon: 'success',
                            title: 'Thêm ca thành công',
                            confirmButtonColor: '#16a34a'
                        }).then(() => location.reload());
                });
        });
    }

    /* ================= EDIT ================= */
    function openEdit(id, name, start, end, late, isActive, type) {

        Swal.fire({
            title: '✏️ Sửa ca làm',
            width: 500,
            html: style() + `
            <div class="swal-form">

                <label>Tên ca</label>
                <input id="shiftName" class="swal2-input" value="${name}">

                <label>Giờ bắt đầu</label>
                <input type="time" id="startTime" class="swal2-input" value="${start}">

                <label>Giờ kết thúc</label>
                <input type="time" id="endTime" class="swal2-input" value="${end}">

                <label>Mốc trễ</label>
                <input type="time" id="lateTime" class="swal2-input" value="${late}">

                <label>Hoạt động</label>
                <input type="checkbox" id="isActive" ${isActive ? "checked" : ""}>
            </div>
        `,
            showCancelButton: true,
            confirmButtonText: 'Cập nhật',

            preConfirm: () => ({
                id: id,
                name: document.getElementById('shiftName').value,
                startTime: document.getElementById('startTime').value,
                endTime: document.getElementById('endTime').value,
                lateThreshold: document.getElementById('lateTime').value,
                isActive: document.getElementById('isActive').checked,
                type: type
            })
        })
            .then(r => {

                if (!r.isConfirmed) return;

                fetch('/Shifts/EditAjax', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(r.value)
                })
                    .then(r => r.json())
                    .then(res => {

                        if (res.success)
                            Swal.fire({
                                icon: 'success',
                                title: 'Cập nhật thành công'
                            }).then(() => location.reload());
                    });
            });
    }

    /* ================= TOGGLE ATTENDANCE ================= */
    function toggleAttendance(id) {

        fetch(`/Shifts/ToggleAttendance?id=${id}`, {
            method: 'POST'
        })
            .then(r => r.json())
            .then(res => {

                if (res.success)
                    location.reload();
            });
    }

    function filterStatus(status) {

        document.querySelectorAll("tbody tr")
            .forEach(row => {

                if (status === "ALL") {
                    row.style.display = "";
                    return;
                }

                const rowStatus = row.dataset.status;

                row.style.display =
                    rowStatus === status ? "" : "none";
            });
    }

    /* ================= TOGGLE ACTIVE ================= */
    function toggleActive(id, type) {
        fetch(`/Shifts/ToggleActive?id=${id}&type=${type}`, {
            method: 'POST'
        }).then(() => location.reload());
    }

    return {
        openCreate,
        openEdit,
        toggleAttendance,
        toggleActive,
        switchTab,
        filterStatus
    };

})();