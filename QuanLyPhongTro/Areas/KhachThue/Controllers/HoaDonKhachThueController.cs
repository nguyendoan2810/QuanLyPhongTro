using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class HoaDonKhachThueController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HoaDonKhachThueController(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReloadHoaDonKhachThue()
        {
            return ViewComponent("HoaDonKhachThue");
        }

        [HttpGet]
        public async Task<IActionResult> LayChiTietHoaDon(int maHd)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                            .ThenInclude(p => p.ChiTietPhong)
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaKhachNavigation)
                    .FirstOrDefaultAsync(h => h.MaHd == maHd);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

                var hopDong = hoaDon.MaHopDongNavigation;
                var phong = hopDong?.MaPhongNavigation;
                var khach = hopDong?.MaKhachNavigation;

                // 🔹 Lấy tiền phòng từ bảng Phòng
                decimal tienPhong = phong?.GiaPhong ?? 0;

                // 🔹 Lấy danh sách chi tiết dịch vụ trong hóa đơn
                var chiTietDv = await _context.ChiTietHoaDons
                    .Include(ct => ct.MaDvNavigation)
                    .Where(ct => ct.MaHd == maHd)
                    .Select(ct => new
                    {
                        TenDv = ct.MaDvNavigation.TenDv,
                        SoLuong = ct.SoLuong ?? 0,
                        DonGia = ct.DonGia,
                        ThanhTien = ct.ThanhTien ?? 0
                    })
                    .ToListAsync();

                // 🔹 Lấy chỉ số điện & nước
                int maHopDong = hopDong?.MaHopDong ?? 0;
                int thang = hoaDon.Thang ?? 0;
                int nam = hoaDon.Nam ?? 0;

                var chiSoDien = await _context.ChiSoDichVus
                    .Where(cs => cs.MaHopDong == maHopDong && cs.Thang == thang && cs.Nam == nam && cs.MaDv == 1)
                    .Select(cs => new { cs.ChiSoCu, cs.ChiSoMoi })
                    .FirstOrDefaultAsync();

                var chiSoNuoc = await _context.ChiSoDichVus
                    .Where(cs => cs.MaHopDong == maHopDong && cs.Thang == thang && cs.Nam == nam && cs.MaDv == 2)
                    .Select(cs => new { cs.ChiSoCu, cs.ChiSoMoi })
                    .FirstOrDefaultAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        hoaDon.MaHd,
                        TenPhong = phong != null ? $"{phong.TenPhong} - {phong.ChiTietPhong?.DiaChi ?? "Chưa có địa chỉ"}" : "Không rõ",
                        TenKhachThue = khach?.HoTen ?? "Không rõ",
                        hoaDon.Thang,
                        hoaDon.Nam,
                        hoaDon.TrangThai,
                        hoaDon.TongTien,
                        hoaDon.NgayTao,
                        TienPhong = tienPhong,
                        ChiTietDichVu = chiTietDv,
                        ChiSoDien = chiSoDien,
                        ChiSoNuoc = chiSoNuoc
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy chi tiết hóa đơn: {ex.Message}" });
            }
        }

    }
}
