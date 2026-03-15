var Department = (function () {

    /* ================= STYLE ================= */
    function style() {
        return `
        <style>
            .swal-form{
                display:grid;
                grid-template-columns:120px 1fr;
                gap:12px;
                align-items:center;
            }
        </style>`;
    }

    /* ================= CREATE ================= */
    function openCreate() {

        Swal.fire({
            title: '➕ Thêm phòng ban',
            html: style() + `
                <div class="swal-form">
                    <label>Mã phòng ban</label>
                    <input id="code" class="swal2-input">

                    <label>Tên phòng ban</label>
                    <input id="name" class="swal2-input">

                    <label>Mô tả phòng ban</label>
                    <input id="desc" class="swal2-input">
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Lưu',

            preConfirm: () => {

                const code = document.getElementById('code').value;
                const name = document.getElementById('name').value;

                if (!code || !name) {
                    Swal.showValidationMessage("Nhập đầy đủ");
                    return false;
                }

                return {
                    departmentCode: code,
                    departmentName: name,
                    description: document.getElementById('desc').value
                };
            }

        }).then(r => {

            if (!r.isConfirmed) return;

            fetch('/Departments/CreateAjax', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(res => {

                    if (res.success)
                        Swal.fire({
                            icon: 'success',
                            title: 'Thêm phòng ban thành công',
                            text: 'Phòng ban mới đã được lưu vào hệ thống và sẵn sàng sử dụng.',
                            confirmButtonText: 'OK',
                            confirmButtonColor: '#2563eb'
                        }).then(() => location.reload());
                    else
                        Swal.fire('❌', res.message, 'error');
                });
        });
    }

    /* ================= EDIT ================= */
    function openEdit(id, code, name, desc) {

        Swal.fire({
            title: '✏️ Sửa phòng ban',
            html: style() + `
                <div class="swal-form">
                    <label>Mã</label>
                    <input id="code" class="swal2-input" value="${code}">

                    <label>Tên</label>
                    <input id="name" class="swal2-input" value="${name}">

                    <label>Mô tả</label>
                    <input id="desc" class="swal2-input" value="${desc ?? ''}">
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Cập nhật',

            preConfirm: () => ({
                id: id,
                departmentCode: document.getElementById('code').value,
                departmentName: document.getElementById('name').value,
                description: document.getElementById('desc').value
            })
        }).then(r => {

            if (!r.isConfirmed) return;

            fetch('/Departments/EditAjax', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(r.value)
            })
                .then(r => r.json())
                .then(res => {
                    if (res.success)
                        Swal.fire({
                            icon: 'success',
                            title: 'Cập nhật thành công',
                            text: 'Thông tin phòng ban đã được cập nhật.',
                            confirmButtonText: 'Đóng',
                            confirmButtonColor: '#16a34a'
                        }).then(() => location.reload());
                });
        });
    }

    /* ================= DELETE ================= */
    function del(id) {

        Swal.fire({
            title: 'Xóa phòng ban?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa'
        }).then(r => {

            if (!r.isConfirmed) return;

            fetch(`/Departments/DeleteAjax?id=${id}`, {
                method: 'POST'
            })
                .then(r => r.json())
                .then(res => {

                    if (res.success)
                        Swal.fire({
                            icon: 'success',
                            title: 'Xóa thành công',
                            text: 'Phòng ban đã được xóa khỏi hệ thống.',
                            confirmButtonText: 'OK',
                            confirmButtonColor: '#dc2626'
                        }).then(() => location.reload());
                    else
                        Swal.fire('❌', res.message, 'error');
                });
        });
    }

    return {
        openCreate,
        openEdit,
        delete: del
    };

})();