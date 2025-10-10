using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class HoaDonController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HoaDonController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LayDanhSachPhongDangThue()
        {
            // Lấy danh sách hợp đồng còn hiệu lực
            var hopDongsConHieuLuc = _context.HopDongs
                .Include(h => h.MaKhachNavigation)
                .Where(h => h.TrangThai == "Còn hiệu lực")
                .ToList();

            // Lấy danh sách phòng đang thuê
            var phongsDangThue = _context.Phongs
                .Include(p => p.ChiTietPhong)
                .Where(p => p.TrangThai == "Đang thuê")
                .ToList();

            // Ghép thông tin phòng với hợp đồng
            var danhSachPhong = (from p in phongsDangThue
                                 join h in hopDongsConHieuLuc on p.MaPhong equals h.MaPhong
                                 select new
                                 {
                                     MaHopDong = h.MaHopDong,
                                     MaPhong = p.MaPhong,
                                     TenPhong = p.TenPhong,
                                     DiaChi = p.ChiTietPhong?.DiaChi ?? "",
                                     GiaPhong = p.GiaPhong,
                                     TenKhach = h.MaKhachNavigation.HoTen,
                                     MaKhach = h.MaKhach
                                 }).ToList();

            return Json(danhSachPhong);
        }

        [HttpGet]
        public IActionResult LayDanhSachDichVu()
        {
            var dichVus = _context.DichVus
                .Select(d => new { d.MaDv, d.TenDv, d.DonGia })
                .ToList();

            return Json(dichVus);
        }

    }
}
