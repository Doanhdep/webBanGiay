using System.ComponentModel.DataAnnotations;

namespace MyStore.myDTO
{
    public class Register_DTO
    {
        [Key]
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [MaxLength(20, ErrorMessage = "Tên đăng nhập không được vượt quá 20 ký tự.")]
        [MinLength(15, ErrorMessage = "Tên đăng nhập phải có ít nhất 15 ký tự.")]
        [RegularExpression(@"^\S{6,20}$", ErrorMessage = "Tên đăng nhập không được chứa khoảng trắng.")]
        public string Username { get; set; } = null!;
      
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;


        [Display(Name = "Số điện thoại")]
        [MaxLength(24, ErrorMessage = "Số điện thoại không được vượt quá 24 ký tự.")]
        [RegularExpression(@"0[987536]\d{8}", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm 1 chữ hoa, 1 chữ số và 1 ký tự đặc biệt.")]
        public string Password { get; set; } = null!;

        [Display(Name = "Xác nhận mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? RandomKey { get; set; }
    }
}
