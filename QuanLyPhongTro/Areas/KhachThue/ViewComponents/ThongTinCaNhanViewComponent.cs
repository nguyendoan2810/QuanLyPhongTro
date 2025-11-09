using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.ViewComponents
{
    public class ThongTinCaNhanViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public ThongTinCaNhanViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var maKhach = HttpContext.Session.GetInt32("MaKhach");
            if (maKhach == null) return View("~/Areas/KhachThue/Views/ThongTinCaNhan/Index.cshtml");

            var khach = _context.KhachThues.FirstOrDefault(k => k.MaKhach == maKhach);
            return View("~/Areas/KhachThue/Views/ThongTinCaNhan/Index.cshtml", khach);
        }
    }
}
