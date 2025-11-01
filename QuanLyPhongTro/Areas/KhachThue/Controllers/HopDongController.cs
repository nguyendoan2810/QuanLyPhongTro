using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class HopDongController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HopDongController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GiaHan(int MaHopDong, int SoThang)
        {
            var hopDong = _context.HopDongs
                .Include(h => h.MaPhongNavigation)
                .ThenInclude(p => p.MaChuTroNavigation)
                .FirstOrDefault(h => h.MaHopDong == MaHopDong);

            if (hopDong == null || hopDong.MaPhongNavigation == null || hopDong.MaPhongNavigation.MaChuTroNavigation == null)
                return BadRequest();

            var thongBao = new ThongBao
            {
                MaKH = hopDong.MaKhach,
                NgayGui = DateTime.Now,
                Luu = $"Yêu cầu gia hạn hợp đồng phòng {hopDong.MaPhongNavigation.TenPhong} tại {hopDong.MaPhongNavigation.ChiTietPhong?.DiaChi} thêm {SoThang} tháng."
            };

            _context.ThongBaos.Add(thongBao);
            _context.SaveChanges();

            return Content("OK");
        }
    }
}
