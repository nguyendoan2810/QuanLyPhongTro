using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Areas.QuanLy.Services;
using QuanLyPhongTro.Models;
using BCrypt.Net;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class AccountController : Controller
    {
        private readonly QuanLyPhongTroContext _context;
        private readonly SendGridService _sendGrid; 

        public AccountController(QuanLyPhongTroContext context, SendGridService sendGrid)
        {
            _context = context;
            _sendGrid = sendGrid;
        }

        public IActionResult Index()
        {
            return View();
        }


        //ĐĂNG NHẬP
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            var user = _context.TaiKhoans.FirstOrDefault(t =>
                (t.TenDangNhap == username || t.Email == username) &&
                t.MaChuTro != null &&
                (t.TrangThai ?? true));

            if (user != null && PasswordHelper.VerifyPassword(password, user.MatKhau))
            {
                // lưu session thông tin đăng nhập
                HttpContext.Session.SetInt32("MaTk", user.MaTk);
                HttpContext.Session.SetString("Username", user.TenDangNhap);
                HttpContext.Session.SetInt32("MaChuTro", user.MaChuTro.Value);
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
                HttpContext.Session.SetString("Email", user.Email ?? "");

                return RedirectToAction("Index", "QuanLyMain", new { area = "QuanLy" });
            }

            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { area = "QuanLy" });
        }

        //QUÊN MẬT KHẨU
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.TaiKhoans.FirstOrDefault(x => x.Email == email && x.MaChuTro != null);
            if (user == null)
                return BadRequest("Không tìm thấy tài khoản với email này.");

            var tempPassword = GenerateTemporaryPassword();
            user.MatKhau = PasswordHelper.HashPassword(tempPassword);
            _context.SaveChanges();

            var subject = "Mật khẩu tạm thời - Quản lý Phòng trọ";
            var body = $"Xin chào {user.HoTen ?? "bạn"},<br><br>Mật khẩu tạm thời của bạn là: <strong>{tempPassword}</strong><br>Vui lòng đăng nhập và đổi lại mật khẩu ngay.";

            try
            {
                await _sendGrid.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi gửi email: " + ex.ToString());
                return BadRequest("Không thể gửi email. Vui lòng thử lại sau.");
            }

            return Json(new { success = true, message = "Mật khẩu tạm thời đã được gửi đến email của bạn." });
        }
        //Tạo mật khẩu tạm thời
        private string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        //ĐĂNG KÝ
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string phone, string cccd, string username, string password, string confirmPassword, bool acceptTerms)
        {
            if (password != confirmPassword)
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp" });

            if (!acceptTerms)
                return Json(new { success = false, message = "Bạn phải đồng ý điều khoản sử dụng" });

            // Số điện thoại: 10 số
            var phoneRegex = new System.Text.RegularExpressions.Regex(@"^\d{10}$");
            if (!phoneRegex.IsMatch(phone))
                return Json(new { success = false, message = "Số điện thoại phải đúng 10 số" });

            // CCCD: 12 số
            var cccdRegex = new System.Text.RegularExpressions.Regex(@"^\d{12}$");
            if (!cccdRegex.IsMatch(cccd))
                return Json(new { success = false, message = "CCCD phải đúng 12 số" });

            // Mật khẩu mạnh: 8–12 ký tự, có chữ hoa + chữ thường + số
            var passwordRegex = new System.Text.RegularExpressions.Regex(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,12}$"
            );
            if (!passwordRegex.IsMatch(password))
                return Json(new
                {
                    success = false,
                    message = "Mật khẩu phải 8–12 ký tự, gồm chữ hoa, chữ thường và số"
                });

            // Kiểm tra username/email đã tồn tại
            if (_context.TaiKhoans.Any(t => t.TenDangNhap == username || t.Email == email))
                return Json(new { success = false, message = "Tên đăng nhập hoặc email đã tồn tại" });

            // Sinh OTP 6 số
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu tạm dữ liệu đăng ký + OTP vào Session
            HttpContext.Session.SetString("Register_FullName", fullName);
            HttpContext.Session.SetString("Register_Email", email);
            HttpContext.Session.SetString("Register_Phone", phone);
            HttpContext.Session.SetString("Register_CCCD", cccd);
            HttpContext.Session.SetString("Register_Username", username);
            HttpContext.Session.SetString("Register_Password", password);
            HttpContext.Session.SetString("Register_OTP", otp);
            HttpContext.Session.SetString("Register_OTP_Expire", DateTime.Now.AddMinutes(5).ToString("O"));

            // Gửi email OTP
            var subject = "Xác nhận đăng ký - Quản lý Phòng trọ";
            var body = $"Xin chào {fullName},<br>Mã xác nhận đăng ký của bạn là: <b>{otp}</b>";
            await _sendGrid.SendEmailAsync(email, subject, body);

            return Json(new { success = true, message = "Mã xác nhận đã được gửi đến email của bạn" });
        }

        // Bước 2: Xác nhận OTP
        [HttpPost]
        public IActionResult ConfirmRegister([FromBody] ConfirmOtpRequest request)
        {
            var otpSession = HttpContext.Session.GetString("Register_OTP");
            if (otpSession == null || otpSession != request.Otp)
                return Json(new { success = false, message = "Mã xác nhận không đúng" });

            var expireStr = HttpContext.Session.GetString("Register_OTP_Expire");
            if (DateTime.TryParse(expireStr, out var expireTime) && DateTime.Now > expireTime)
                return Json(new { success = false, message = "Mã OTP đã hết hạn. Vui lòng đăng ký lại." });

            // Lấy dữ liệu đăng ký từ session
            var fullName = HttpContext.Session.GetString("Register_FullName");
            var email = HttpContext.Session.GetString("Register_Email");
            var phone = HttpContext.Session.GetString("Register_Phone");
            var cccd = HttpContext.Session.GetString("Register_CCCD");
            var username = HttpContext.Session.GetString("Register_Username");
            var password = HttpContext.Session.GetString("Register_Password");

            // Kiểm tra dữ liệu trước khi lưu
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return Json(new { success = false, message = "Dữ liệu đăng ký không hợp lệ." });

            try
            {
                // Tạo Chủ trọ trước
                var chuTro = new ChuTro
                {
                    HoTen = fullName,
                    Email = email,
                    SoDienThoai = phone,
                    Cccd = cccd
                };
                _context.ChuTros.Add(chuTro);
                _context.SaveChanges();

                // Kiểm tra lại MaChuTro sau khi lưu
                if (chuTro.MaChuTro == 0)
                    return Json(new { success = false, message = "Không thể tạo mã chủ trọ. Vui lòng thử lại." });

                // Tạo Tài khoản gắn với chủ trọ
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhau = PasswordHelper.HashPassword(password),
                    VaiTro = "Admin".Trim(),
                    HoTen = fullName,
                    Email = email,
                    SoDienThoai = phone,
                    TrangThai = true,
                    MaChuTro = chuTro.MaChuTro,
                    Cccd = cccd
                };
                _context.TaiKhoans.Add(taiKhoan);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu tài khoản: " + ex);
                if (ex.InnerException != null)
                    Console.WriteLine("Chi tiết: " + ex.InnerException.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return Json(new { success = false, message = "Đăng ký thất bại. Vui lòng thử lại." });
            }

            // Xóa toàn bộ session đăng ký
            string[] keys = {"Register_FullName", "Register_Email", "Register_Phone",
                             "Register_CCCD", "Register_Username", "Register_Password",
                             "Register_OTP", "Register_OTP_Expire"};

            foreach (var key in keys)
                HttpContext.Session.Remove(key);

            return Json(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập." });
        }
    }

    public class ConfirmOtpRequest
    {
        public string Otp { get; set; }
    }

    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashed)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashed);
        }
    }

}