using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class PhongController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public PhongController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ✅ Action xem chi tiết phòng (có phân quyền theo chủ trọ)
        //public IActionResult ChiTietPhong(int maPhong)
        //{
        //    var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
        //    if (maChuTro == null) return Unauthorized();

        //    var phong = _context.Phongs.FirstOrDefault(p => p.MaPhong == maPhong && p.MaChuTro == maChuTro);
        //    if (phong == null) return Unauthorized();

        //    return View(phong);
        //}
    }
}