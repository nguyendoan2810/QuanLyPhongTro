using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.ViewComponents
{
    public class ThongBaoLienHeViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public ThongBaoLienHeViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maTk = HttpContext.Session.GetInt32("MaTk");
            List<ThongBao> danhSachThongBao = new();

            if (maTk != null)
            {
                danhSachThongBao = await _context.ThongBaos
                    .Where(tb => tb.MaTk == maTk)
                    .OrderByDescending(tb => tb.NgayGui)
                    .ToListAsync();
            }

            return View("~/Areas/KhachThue/Views/ThongBaoLienHe/Index.cshtml", danhSachThongBao);
        }
    }
}