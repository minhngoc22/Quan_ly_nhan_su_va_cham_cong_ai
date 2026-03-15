var User = (function () {

    /* ================= TAB SWITCH ================= */
    function switchTab(event, tabId) {

        document.querySelectorAll(".tab-content")
            .forEach(t => t.classList.remove("active"));

        document.querySelectorAll(".tab-btn")
            .forEach(b => b.classList.remove("active"));

        document.getElementById(tabId)
            ?.classList.add("active");

        event.currentTarget.classList.add("active");
    }

    /* ================= APPLY FILTER ================= */
    function applyFilter() {

        const keyword = document.getElementById("searchUser")?.value ?? "";
        const role = document.getElementById("filterRole")?.value ?? "";
        const status = document.getElementById("filterStatus")?.value ?? "";

        const url =
            `/Users/Index?keyword=${encodeURIComponent(keyword)}`
            + `&role=${role}`
            + `&isActive=${status}`;

        window.location.href = url;
    }

    /* ================= RESET FACE ================= */
    async function resetFace(id) {

        const confirm = await Swal.fire({
            title: "Reset dữ liệu khuôn mặt?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Reset"
        });

        if (!confirm.isConfirmed) return;

        try {

            const token =
                document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const res = await fetch("/Users/ResetFace", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify({ id })
            });

            const data = await res.json();

            if (data.success) {
                Swal.fire("Thành công", "Đã reset Face", "success");
            } else {
                Swal.fire("Thất bại", "Reset không thành công", "error");
            }
        }
        catch (e) {
            console.error(e);
            Swal.fire("Lỗi server", "", "error");
        }
    }

    /* ================= DELETE USER ================= */
    async function deleteUser(id) {

        const confirm = await Swal.fire({
            title: "Xóa tài khoản này?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Xóa"
        });

        if (!confirm.isConfirmed) return;

        try {

            const token =
                document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const res = await fetch(`/Users/Delete/${id}`, {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token
                }
            });

            const data = await res.json();

            if (data.success) {
                location.reload();
            } else {
                Swal.fire("Thất bại", "Không thể xóa", "error");
            }
        }
        catch (err) {
            console.error(err);
            Swal.fire("Lỗi server", "", "error");
        }
    }

    /* ================= OPEN EDIT MODAL ================= */
    function openEdit(id, role, status) {

        document.getElementById("editUserId").value = id;

        if (role.includes(",")) {
            role = role.split(",")[0];
        }

        document.getElementById("editRole").value = role;
        document.getElementById("editStatus").value = status;

        const modal =
            new bootstrap.Modal(document.getElementById("editUserModal"));

        modal.show();
    }

    /* ================= SAVE EDIT ================= */
    async function saveEdit() {

        const id = document.getElementById("editUserId").value;
        const role = document.getElementById("editRole").value;
        const status = document.getElementById("editStatus").value;

        const token =
            document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const body = {
            userId: parseInt(id),
            roleName: role || null,
            isActive: status === "" ? null : status === "true"
        };

        try {

            Swal.fire({
                title: "Đang cập nhật...",
                allowOutsideClick: false,
                didOpen: () => Swal.showLoading()
            });

            const res = await fetch("/Users/UpdateUser", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify(body)
            });

            const data = await res.json();
            Swal.close();

            if (data.success) {

                await Swal.fire({
                    icon: "success",
                    title: "Cập nhật thành công",
                    timer: 1200,
                    showConfirmButton: false
                });

                location.reload();
            }
            else {
                Swal.fire("Thất bại", "Không thể cập nhật", "error");
            }
        }
        catch (e) {
            console.error(e);
            Swal.close();
            Swal.fire("Lỗi server", "", "error");
        }
    }

    /* ================= EVENTS ================= */
    function initEvents() {

        document.getElementById("searchUser")
            ?.addEventListener("keyup", debounce(applyFilter, 400));

        document.getElementById("filterRole")
            ?.addEventListener("change", applyFilter);

        document.getElementById("filterStatus")
            ?.addEventListener("change", applyFilter);
    }

    /* ================= DEBOUNCE ================= */
    function debounce(func, delay) {
        let timer;
        return function () {
            clearTimeout(timer);
            timer = setTimeout(func, delay);
        };
    }

    /* ================= PUBLIC ================= */
    return {
        init: initEvents,
        switchTab: switchTab,
        resetFace: resetFace,
        delete: deleteUser,
        openEdit: openEdit,
        saveEdit: saveEdit
    };

})();

/* ================= AUTO INIT ================= */
document.addEventListener("DOMContentLoaded", function () {
    User.init();
});