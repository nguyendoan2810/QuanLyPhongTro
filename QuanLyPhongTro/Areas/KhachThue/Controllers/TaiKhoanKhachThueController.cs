using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class TaiKhoanKhachThueController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public TaiKhoanKhachThueController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        // API xử lý đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest req)
        {
            try
            {
                var maTk = HttpContext.Session.GetInt32("MaTk");
                if (maTk == null)
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

                if (string.IsNullOrWhiteSpace(req.MatKhauCu) || string.IsNullOrWhiteSpace(req.MatKhauMoi) || string.IsNullOrWhiteSpace(req.XacNhan))
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

                if (req.MatKhauMoi != req.XacNhan)
                    return Json(new { success = false, message = "Mật khẩu mới và xác nhận không khớp." });

                var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTk == maTk);
                if (taiKhoan == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản." });

                if (!BCrypt.Net.BCrypt.Verify(req.MatKhauCu, taiKhoan.MatKhau))
                    return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });

                taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(req.MatKhauMoi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }

    public class DoiMatKhauRequest
    {
        public string? MatKhauCu { get; set; }
        public string? MatKhauMoi { get; set; }
        public string? XacNhan { get; set; }
    }
}