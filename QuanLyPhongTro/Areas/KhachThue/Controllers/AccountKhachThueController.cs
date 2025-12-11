using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Areas.QuanLy.Controllers;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class AccountKhachThueController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public AccountKhachThueController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            var user = _context.TaiKhoans
                .Include(t => t.MaKhachNavigation)
                .FirstOrDefault(t =>
                    t.TenDangNhap == username &&
                    t.MaKhach != null &&
                    (t.TrangThai ?? true));

            if (user != null && PasswordHelper.VerifyPassword(password, user.MatKhau))
            {
                var hoTenKhach = user.MaKhachNavigation?.HoTen ?? "Khách thuê";

                HttpContext.Session.SetInt32("MaTk", user.MaTk);
                HttpContext.Session.SetString("Username", user.TenDangNhap);
                HttpContext.Session.SetInt32("MaKhach", user.MaKhach.Value);
                HttpContext.Session.SetString("HoTen", hoTenKhach);

                return RedirectToAction("Index", "KhachThueMain", new { area = "KhachThue" });
            }

            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu!");
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ session
            return RedirectToAction("Login", "AccountKhachThue", new { area = "KhachThue" });
        }
    }
}