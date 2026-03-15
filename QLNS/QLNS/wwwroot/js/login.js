// ====================== LOGIN SUCCESS REDIRECT ======================
if (window.loginConfig) {
    const { successMessage, redirectUrl } = window.loginConfig;

    if (successMessage && redirectUrl) {
        Swal.fire({
            title: 'Thành công 🎉',
            text: successMessage,
            icon: 'success',
            confirmButtonText: 'Vào hệ thống',
            confirmButtonColor: '#0d6efd',
            allowOutsideClick: false
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = redirectUrl;
            }
        });
    }
}

// ====================== ROLE SELECT ======================
function selectRole(role) {
    document.getElementById('roleStep').style.display = 'none';
    document.getElementById('loginStep').style.display = 'block';

    document.getElementById('selectedRole').value = role;
    document.getElementById('selectedRoleText').innerText = role;

    const userInput = document.querySelector('input[name="username"]');
    if (userInput) userInput.focus();
}

function backToRole() {
    document.querySelector('form').reset();
    document.getElementById('loginStep').style.display = 'none';
    document.getElementById('roleStep').style.display = 'block';
}

// ====================== STAY ON LOGIN WHEN ERROR ======================
document.addEventListener('DOMContentLoaded', function () {
    if (window.loginState?.stayOnLogin && window.loginState?.selectedRole) {
        document.getElementById('roleStep').style.display = 'none';
        document.getElementById('loginStep').style.display = 'block';

        document.getElementById('selectedRole').value = window.loginState.selectedRole;
        document.getElementById('selectedRoleText').innerText = window.loginState.selectedRole;
    }

    // ====================== CAPS LOCK WARNING ======================
    const passwordInput = document.querySelector('input[name="password"]');
    const capsWarning = document.getElementById('capsWarning');

    if (passwordInput && capsWarning) {
        passwordInput.addEventListener('keyup', function (e) {
            if (e.getModifierState && e.getModifierState('CapsLock')) {
                capsWarning.classList.remove('d-none');
            } else {
                capsWarning.classList.add('d-none');
            }
        });
    }
});
