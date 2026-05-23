using System.ComponentModel.DataAnnotations;

namespace MyStore.myDTO
{
    public class Login_DTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 6 đến 20 ký tự.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Tên đăng nhập không được chứa khoảng trắng.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_])(?!.*\s).{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ số, 1 ký tự đặc biệt và không có khoảng trắng.")]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; } // ✅ Giữ đăng nhập
    }
}
