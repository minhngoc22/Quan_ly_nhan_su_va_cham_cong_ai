using System.ComponentModel.DataAnnotations;

namespace QLNS.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
