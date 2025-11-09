using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.ViewComponents
{
    public class HoaDonKhachThueViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public HoaDonKhachThueViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maKhach = HttpContext.Session.GetInt32("MaKhach");
            if (maKhach == null)
            {
                return View("~/Areas/KhachThue/Views/HoaDonKhachThue/Index.cshtml", new List<HoaDon>());
            }

            // Lấy hợp đồng hiện tại (hợp đồng còn hiệu lực, mới nhất)
            var hopDong = await _context.HopDongs
                .Where(h => h.MaKhach == maKhach && h.TrangThai != "Đã kết thúc")
                .OrderByDescending(h => h.NgayBatDau)
                .FirstOrDefaultAsync();

            if (hopDong == null)
            {
                return View("~/Areas/KhachThue/Views/HoaDonKhachThue/Index.cshtml", new List<HoaDon>());
            }

            var hoaDons = await _context.HoaDons
                .Where(h => h.MaHopDong == hopDong.MaHopDong)
                .Include(h => h.MaHopDongNavigation)
                .OrderByDescending(h => h.Nam)
                .ThenByDescending(h => h.Thang)
                .ToListAsync();

            return View("~/Areas/KhachThue/Views/HoaDonKhachThue/Index.cshtml", hoaDons);
        }
    }
}