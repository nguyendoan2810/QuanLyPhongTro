using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.ViewComponents
{
    public class HopDongViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public HopDongViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maKhach = HttpContext.Session.GetInt32("MaKhach");
            if (maKhach == null)
                return View("~/Areas/KhachThue/Views/HopDong/Index.cshtml");

            var hopDongs = await _context.HopDongs
                .Include(h => h.MaKhachNavigation)
                .Include(h => h.MaPhongNavigation)
                .ThenInclude(p => p.ChiTietPhong)
                .Where(h => h.MaKhach == maKhach)
                .ToListAsync();

            return View("~/Areas/KhachThue/Views/HopDong/Index.cshtml", hopDongs);
        }
    }
}
