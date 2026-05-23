using System.ComponentModel.DataAnnotations;

namespace MyStore.myDTO
{
    public class ForgotPassword_DTO
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }

}
