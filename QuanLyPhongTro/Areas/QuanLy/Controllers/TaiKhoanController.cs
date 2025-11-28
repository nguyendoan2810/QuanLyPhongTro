using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public TaiKhoanController(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            var maTk = HttpContext.Session.GetInt32("MaTk");
            if (maTk == null)
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

            if (string.IsNullOrWhiteSpace(matKhauCu) || string.IsNullOrWhiteSpace(matKhauMoi) || string.IsNullOrWhiteSpace(xacNhanMatKhau))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            if (matKhauMoi != xacNhanMatKhau)
                return Json(new { success = false, message = "Mật khẩu mới và xác nhận không khớp." });

            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaTk == maTk);
            if (taiKhoan == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            if (!BCrypt.Net.BCrypt.Verify(matKhauCu, taiKhoan.MatKhau))
                return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });

            taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        // LẤY DANH SÁCH PHÒNG CÓ KHÁCH CHƯA CÓ TÀI KHOẢN & HỢP ĐỒNG CÒN HIỆU LỰC
        [HttpGet]
        public IActionResult GetAvailableRooms()
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            // Lọc các hợp đồng của chủ trọ có trạng thái "Còn hiệu lực"
            var danhSach = _context.HopDongs
                .Include(h => h.MaKhachNavigation)
                .Include(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.ChiTietPhong)
                .Where(h =>
                    h.MaPhongNavigation.MaChuTro == maChuTro &&
                    h.TrangThai == "Còn hiệu lực" &&
                    !_context.TaiKhoans.Any(t => t.MaKhach == h.MaKhach && t.VaiTro == "Khach")
                )
                .Select(h => new
                {
                    MaPhong = h.MaPhong,
                    TenPhong = h.MaPhongNavigation.TenPhong,
                    DiaChi = h.MaPhongNavigation.ChiTietPhong != null
                        ? h.MaPhongNavigation.ChiTietPhong.DiaChi
                        : "",
                    MaKhach = h.MaKhach,
                    TenKhach = h.MaKhachNavigation.HoTen
                })
                .Distinct()
                .ToList();

            return Json(new { success = true, data = danhSach });
        }

        // THÊM TÀI KHOẢN KHÁCH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemTaiKhoanKhach(int maPhong, string tenDangNhap, string matKhau)
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            // Tìm hợp đồng và khách tương ứng
            var hopDong = _context.HopDongs
                .Include(h => h.MaKhachNavigation)
                .FirstOrDefault(h => h.MaPhong == maPhong && h.TrangThai == "Còn hiệu lực");

            if (hopDong == null || hopDong.MaKhach == null)
                return Json(new { success = false, message = "Không tìm thấy hợp đồng cho phòng này." });

            if (_context.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap))
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại." });

            // Mã hóa mật khẩu
            var hash = BCrypt.Net.BCrypt.HashPassword(matKhau);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = hash,
                VaiTro = "Khach",
                MaKhach = hopDong.MaKhach,
                TrangThai = true,
            };

            _context.TaiKhoans.Add(taiKhoan);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thêm tài khoản khách thành công!" });
        }

        // Trả về ViewComponent để reload danh sách tài khoản (AJAX)
        [HttpGet]
        public IActionResult ReloadPartial()
        {
            return ViewComponent("TaiKhoan");
        }

        // ===== LẤY THÔNG TIN TÀI KHOẢN KHÁCH =====
        [HttpGet]
        public IActionResult GetTaiKhoanKhach(int id)
        {
            var tk = _context.TaiKhoans
                .FirstOrDefault(t => t.MaTk == id && t.VaiTro == "Khach");

            if (tk == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = tk.MaTk,
                    tenDangNhap = tk.TenDangNhap
                }
            });
        }

        // ===== CẬP NHẬT MẬT KHẨU KHÁCH =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaTaiKhoanKhach(int id, string matKhauMoi, string xacNhanMatKhau)
        {
            if (string.IsNullOrWhiteSpace(matKhauMoi) || string.IsNullOrWhiteSpace(xacNhanMatKhau))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            if (matKhauMoi != xacNhanMatKhau)
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." });

            var tk = _context.TaiKhoans.FirstOrDefault(t => t.MaTk == id && t.VaiTro == "Khach");
            if (tk == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            tk.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
            _context.SaveChanges();

            return Json(new { success = true, message = "Cập nhật mật khẩu thành công!" });
        }

        // ===== XÓA TÀI KHOẢN KHÁCH =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTaiKhoanKhach(int id)
        {
            var tk = _context.TaiKhoans
                .Include(t => t.ThongBaos) // 🔹 Load danh sách Thông báo của tài khoản
                .FirstOrDefault(t => t.MaTk == id && t.VaiTro == "Khach");

            if (tk == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            // 🔹 Xóa tất cả thông báo liên quan trước
            if (tk.ThongBaos != null && tk.ThongBaos.Any())
            {
                _context.ThongBaos.RemoveRange(tk.ThongBaos);
            }

            // 🔹 Sau đó xóa tài khoản
            _context.TaiKhoans.Remove(tk);
            _context.SaveChanges();

            return Json(new { success = true, message = "Xóa tài khoản và các thông báo liên quan thành công!" });
        }
    }
}