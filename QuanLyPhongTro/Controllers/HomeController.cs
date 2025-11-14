using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HomeController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        // ✅ Trang quảng bá phòng
        public async Task<IActionResult> Index(decimal? giaTu, decimal? giaDen)
        {
            var query = _context.Phongs
                .Include(p => p.ChiTietPhong)
                .Include(p => p.MaChuTroNavigation)
                .Where(p => p.TrangThai == "Trống");

            // Lọc theo giá nếu có giá trị
            if (giaTu.HasValue)
                query = query.Where(p => p.GiaPhong >= giaTu.Value);
            if (giaDen.HasValue)
                query = query.Where(p => p.GiaPhong <= giaDen.Value);

            var danhSachPhong = await query.OrderBy(p => p.GiaPhong).ToListAsync();

            // Truyền lại giá lọc để giữ giá trị trên giao diện
            ViewBag.GiaTu = giaTu;
            ViewBag.GiaDen = giaDen;

            return View(danhSachPhong);
        }
    }
}