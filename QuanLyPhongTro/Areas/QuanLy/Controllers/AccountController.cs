using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class AccountController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public AccountController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

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

            // kiểm tra DB, chỉ cho phép tài khoản có MaChuTro != null
            var user = _context.TaiKhoans.FirstOrDefault(t =>
                t.TenDangNhap == username &&
                t.MatKhau == password &&
                t.MaChuTro != null &&            
                (t.TrangThai ?? true));          // vẫn còn hoạt động

            if (user != null)
            {
                // lưu session thông tin đăng nhập
                HttpContext.Session.SetInt32("MaTk", user.MaTk);
                HttpContext.Session.SetString("Username", user.TenDangNhap);
                HttpContext.Session.SetInt32("MaChuTro", user.MaChuTro.Value);

                // lưu thêm họ tên và email để hiển thị sidebar
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
                HttpContext.Session.SetString("Email", user.Email ?? "");

                return RedirectToAction("Index", "QuanLyMain", new { area = "QuanLy" });
            }

            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");

            return View();
        }

        [HttpGet]
        public IActionResult Register() => View(); // return trang Register.cshtml

        [HttpPost]
        public IActionResult Register(string fullName, string email, string phone,
                                 string password, string confirmPassword, bool acceptTerms)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                return View();
            }

            // Lưu thông tin đăng ký vào DB ở đây
            // …

            // Sau khi đăng ký thành công chuyển về trang Login
            return RedirectToAction("Login", "Account", new { area = "QuanLy" });
        }
    }
}
