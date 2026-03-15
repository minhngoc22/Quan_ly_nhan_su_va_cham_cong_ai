// =========================
// EMPLOYEE CREATE MODULE
// =========================
window.EmployeeCreate = {

    // PREVIEW EMPLOYEE CODE
    updateEmployeeCode() {
        const dept = document.querySelector("#DepartmentId option:checked");
        const pos = document.querySelector("#PositionId option:checked");

        if (!dept || !pos) return;

        const deptCode = dept.dataset.code;
        const posCode = pos.dataset.code;

        if (!deptCode || !posCode) return;

        document.getElementById("EmployeeCode").value =
            `${deptCode}-${posCode}-XXX`;
    },

    // PREVIEW AVATAR
    previewAvatar(input) {
        if (!input.files || !input.files[0]) return;

        const reader = new FileReader();
        reader.onload = e => {
            const img = document.getElementById("avatarPreview");
            if (img) img.src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    },

    showCreateUserModal() {
        if (!window.CREATE_USER_SUCCESS) return;

        Swal.fire({
            title: '🎉 Tạo nhân viên thành công',
            html: `
            <p>👤 <b>${window.CREATE_USERNAME}</b></p>
            <p>🔑 <b>${window.CREATE_PASSWORD}</b></p>
        `,
            icon: 'success',
            confirmButtonText: 'OK',
            allowOutsideClick: false
        }).then(result => {
            if (result.isConfirmed && window.CREATE_REDIRECT_URL) {
                window.location.href = window.CREATE_REDIRECT_URL;
            }
        });
    }

};

// AUTO RUN
document.addEventListener("DOMContentLoaded", function () {
    EmployeeCreate.showCreateUserModal();
});
