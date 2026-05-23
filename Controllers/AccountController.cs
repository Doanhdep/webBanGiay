using MailKit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using MyStore.Data;
using MyStore.Helpers;
using MyStore.Models;
using MyStore.myDTO;
using MyStore.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;


namespace MyStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyStoreContext db;
        private readonly MyStore.Services.IMailService _mailService;


        public AccountController(MyStoreContext context, MyStore.Services.IMailService mailService)
        {
            db = context;
            _mailService = mailService;
        }


        [HttpGet]
        public IActionResult VerifyOTP()
        {
            // session 
            string username = HttpContext.Session.GetString("Username") ?? "nguyenhuuhao9999";
            string email = HttpContext.Session.GetString("Email") ?? "nguyenhaohuu9@gmail.com";
            // check 
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            ViewBag.Username = username;
            ViewBag.Email = email;

            return View();
        }

        // Encapsulation
        private string MaskEmail(string email)
        {
            int index = email.IndexOf('@');
            if (index <= 2) return email;

            string prefix = email.Substring(0, 2);
            string domain = email.Substring(index);
            string maskedPart = new string('*', index - 2);

            return prefix + maskedPart + domain;
        }

        // ajax sendotp 
        [HttpPost]
        public IActionResult SendOTP(string username)
        {
            Random random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            user.OtpCode = otpCode;
            user.OtpExpiry = DateTime.Now.AddMinutes(2);
            db.SaveChanges();

            string emailBody = $@"
                    <!DOCTYPE html>
                    <html lang='vi'>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Xác nhận OTP</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; text-align: center; }}
                            .otp {{ font-size: 24px; font-weight: bold; color: red; padding: 10px; }}
                        </style>
                    </head>
                    <body>
                        <h2>Xin chào {user.Username}!</h2>
                        <p>Mã OTP của bạn là:</p>
                        <div class='otp'>{otpCode}</div>
                        <p>Mã này có hiệu lực trong 2 phút.</p>
                    </body>
                    </html>";

            MailData mailData = new MailData
            {
                EmailToId = user.Email,
                EmailSubject = "OTP Verification",
                EmailBody = emailBody,
                EmailToName = user.Username
            };

            bool result = _mailService.SendMail(mailData);
            if (result)
            {
                return Json(new { success = true, message = "OTP đã được gửi lại.", expiry = user.OtpExpiry });
            }

            return Json(new { success = false, message = "Không thể gửi OTP." });
        }

        [HttpPost]
        public JsonResult CheckOTP(string otp)
        {
            string username = HttpContext.Session.GetString("Username") ?? "nguyenhuuhao9999";
            if (username == null)
            {
                return Json(new { success = false, message = " Tài khoản không tồn tại hoặc đã bị xóa khỏi hệ thống " });
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || user.OtpCode != otp || user.OtpExpiry < DateTime.Now)
            {
                return Json(new { success = false, message = "OTP không hợp lệ hoặc đã hết hạn." });
            }

            return Json(new { success = true, redirectUrl = Url.Action("register", "Account") });
        }


        [HttpGet]

        public IActionResult Register()
        {
            return View();

        }
        [HttpPost]
        public IActionResult Register(Register_DTO model)
        {
            var existingUser = db.Users.FirstOrDefault(kh => kh.Username == model.Username);
            if (existingUser != null)
            {

                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại ");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var User = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        RandomKey = MyUtil.GenerateRamdomKey(),
                        Password = model.Password.ToMd5Hash(MyUtil.GenerateRamdomKey()), // Hash mật khẩu
                        Active = false,
                        RoleId = 2,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Thêm vào database
                    db.Add(User);
                    db.SaveChanges();
                    HttpContext.Session.SetString("Username", model.Username);
                    HttpContext.Session.SetString("Email", model.Email);
                    return RedirectToAction("verfiOTP", "Acount");

                }
                catch (Exception ex)
                {
                    var mess = $"{ex.Message} s";
                }
            }

            return View();
        }




        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
       
        public async Task<IActionResult> Login(Login_DTO model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Thông tin đăng nhập không hợp lệ!" });
                }
                return View(model);
            }

            var user = db.Users
                .Where(kh => kh.Username == model.Username)
                .Select(kh => new
                {
                    kh.Id,
                    kh.Username,
                    kh.Password,
                    kh.RandomKey,
                    kh.Active
                })
                .SingleOrDefault();

            if (user == null)
            {
                var errorMsg = "Tài khoản không tồn tại.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                ModelState.AddModelError(string.Empty, errorMsg);
                return View(model);
            }

            if (!user.Active)
            {
                var errorMsg = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                ModelState.AddModelError(string.Empty, errorMsg);
                return View(model);
            }

            // So sánh mật khẩu đã mã hóa MD5
            if (user.Password != model.Password.ToMd5Hash(user.RandomKey))
            {
                var errorMsg = "Sai mật khẩu.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                ModelState.AddModelError(string.Empty, errorMsg);
                return View(model);
            }

            // 🔹 Tạo danh sách claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, "Customer")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe, // ✅ Lưu cookie nếu chọn "Ghi nhớ đăng nhập"
                ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1) // ⏳ Cookies tồn tại 7 ngày nếu "Remember Me", ngược lại là 1 giờ
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // ✅ Lưu UserId vào Session
            HttpContext.Session.SetInt32("UserId", user.Id);
            Console.WriteLine($"✅ Đăng nhập thành công - UserId đã lưu vào Session: {user.Id}");

            // ✅ Trả về JSON nếu request là AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Address") : returnUrl });
            }

            // 🔹 Chuyển hướng nếu không phải AJAX
            return string.IsNullOrWhiteSpace(returnUrl) ? RedirectToAction("Index", "Address") : LocalRedirect(returnUrl);
        }



        [HttpPost] // Chỉ cho phép POST request
        [ValidateAntiForgeryToken] // Bảo mật chống CSRF
        public IActionResult Logout()
        {
            
             HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


        // 🔹 GET: Profile (Chỉ cho phép người dùng đã đăng nhập)
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // ✅ Lấy ID từ Claims

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
            {
                return RedirectToAction("Login");
            }

            var userProfile = db.Users
                .Where(u => u.Id == parsedUserId)
                .Select(u => new User_DTO
                {
                    Id = u.Id,
                    Username = u.Username,          
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Filename = u.Filename, // Avatar
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefault();

            if (userProfile == null)
            {
                return RedirectToAction("Login");
            }

            return View(userProfile);
        }






        [HttpPost]
        public IActionResult SendOTPForForgot(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            // Tạo mã OTP
            string otpCode = new Random().Next(100000, 999999).ToString();

            //Lưu OTP vào Session
            HttpContext.Session.SetString("ResetPasswordOTP", otpCode);
            HttpContext.Session.SetString("ResetPasswordEmail", email);
            HttpContext.Session.SetString("ResetPasswordExpiry", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            //Gửi Email OTP
            string emailBody = $"Mã OTP của bạn là: <b>{otpCode}</b>. Mã có hiệu lực trong 3 phút.";
            MailData mailData = new MailData
            {
                EmailToId = user.Email,
                EmailSubject = "Xác thực đặt lại mật khẩu",
                EmailBody = emailBody,
                EmailToName = user.Username
            };

            if (_mailService.SendMail(mailData))
            {
                return RedirectToAction("VerifyOTPView");
            }

            return Json(new { success = false, message = "Không thể gửi OTP." });
        }




        //Hiển thị form nhập Email để quên mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPassword_DTO());
        }

        //Xử lý gửi OTP qua email
        [HttpPost]

        //Hiển thị form nhập OTP
        [HttpGet]
        public IActionResult VerifyOTPView()
        {
            return View();
        }

        // Xác minh mã OTP
        public class OTPRequestModel
        {
            public string Otp { get; set; }
        }

        [HttpPost]
        public IActionResult VerifyOTP([FromBody] OTPRequestModel model)
        {
            Console.WriteLine($"🔹 OTP nhận vào từ request: {model.Otp ?? "NULL"}");

            if (string.IsNullOrEmpty(model.Otp))
            {
                return Json(new { success = false, message = "OTP không được để trống!" });
            }

            string? storedOtp = HttpContext.Session.GetString("ResetPasswordOTP");
            string? email = HttpContext.Session.GetString("ResetPasswordEmail"); // Lấy email từ session

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "OTP đã hết hạn hoặc không hợp lệ." });
            }

            if (storedOtp.Trim() != model.Otp.Trim())
            {
                return Json(new { success = false, message = "OTP không đúng." });
            }

            // Lưu email vào session để ResetPassword sử dụng
            HttpContext.Session.SetString("VerifiedResetEmail", email);

            // Xóa OTP sau khi xác thực thành công
            HttpContext.Session.Remove("ResetPasswordOTP");
            HttpContext.Session.Remove("ResetPasswordExpiry");

            // 🔹 Trả về URL của ResetPassword View
            return Json(new { success = true, redirectUrl = Url.Action("ResetPassword", "Account") });
        }




        // Hiển thị form đặt lại mật khẩu
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("VerifiedResetEmail")))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        // Xử lý đặt lại mật khẩu 
        [HttpPost]

        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.NewPassword))
            {
                return Json(new { success = false, message = "Mật khẩu mới không được để trống." });
            }

            string? email = HttpContext.Session.GetString("VerifiedResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Phiên đặt lại mật khẩu không hợp lệ." });
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            user.Password = model.NewPassword.ToMd5Hash(user.RandomKey);
            db.SaveChanges();

            HttpContext.Session.Remove("VerifiedResetEmail");

            return Json(new { success = true, message = "Mật khẩu đã được đặt lại thành công." });
        }

        public async Task LoginByGoogle(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse", new { returnUrl })
                });
        }

        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return RedirectToAction("Login");

            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            // ✅ Kiểm tra xem User đã tồn tại trong Database chưa
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                var randomKey = MyUtil.GenerateRamdomKey();
                var randomPassword = "GoogleAuth".ToMd5Hash(randomKey); // Hash chuỗi mặc định

                user = new User
                {
                    Username = email.Split('@')[0], // Lấy phần trước @ làm username
                    Email = email,
                    Password = randomPassword, // Không để null
                    RandomKey = randomKey,
                    Active = true,
                    RoleId = 2,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();
            }


            // ✅ Tạo Claims cho User
            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, "Customer") // Role nếu có
    };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // ✅ Lưu UserId vào Session
            HttpContext.Session.SetInt32("UserId", user.Id);

            return LocalRedirect(returnUrl);
        }

    }
}
