using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password, bool rememberMe)
        {
            // kiểm tra tài khoản, ở đây tạm thời chỉ redirect cho bạn
            if (username == "admin" && password == "123")
            {
                // lưu session/cookie ở đây nếu muốn
                return RedirectToAction("Index", "Dashboard", new { area = "QuanLy" });
            }

            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
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
