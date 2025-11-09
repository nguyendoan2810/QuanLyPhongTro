using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class ThongTinCaNhanController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public ThongTinCaNhanController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatThongTin([FromBody] QuanLyPhongTro.Models.KhachThue model)
        {
            var maKhach = HttpContext.Session.GetInt32("MaKhach");
            if (maKhach == null)
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });

            var khach = await _context.KhachThues.FirstOrDefaultAsync(k => k.MaKhach == maKhach);
            if (khach == null)
                return Json(new { success = false, message = "Không tìm thấy khách thuê." });

            // ✅ Cập nhật thông tin
            khach.HoTen = model.HoTen;
            khach.Cccd = model.Cccd;
            khach.SoDienThoai = model.SoDienThoai;
            khach.DiaChi = model.DiaChi;
            khach.NgaySinh = model.NgaySinh;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
        }
    }
}
