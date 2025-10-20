using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Areas.QuanLy.Models.ViewModels;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class HoaDonViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public HoaDonViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string trangThai = "", int? thang = null, int? nam = null)
        {
            var currentYear = DateTime.Now.Year;

            // Chỉ gán mặc định cho năm nếu chưa có
            nam ??= currentYear;

            // Lấy danh sách hóa đơn
            var hoaDonsQuery = _context.HoaDons
                .Include(hd => hd.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaPhongNavigation)
                .Include(hd => hd.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaKhachNavigation)
                .AsQueryable();

            // ✅ Áp dụng điều kiện lọc
            if (!string.IsNullOrEmpty(trangThai))
            {
                hoaDonsQuery = hoaDonsQuery.Where(hd => hd.TrangThai == trangThai);
            }

            if (thang.HasValue)
            {
                hoaDonsQuery = hoaDonsQuery.Where(hd => hd.Thang == thang.Value);
            }

            if (nam.HasValue)
            {
                hoaDonsQuery = hoaDonsQuery.Where(hd => hd.Nam == nam.Value);
            }

            var hoaDons = await hoaDonsQuery.ToListAsync();

            var hoaDonList = hoaDons.Select(hd => new HoaDonViewModel
            {
                MaHd = hd.MaHd,
                MaHoaDonHienThi = $"HĐ{hd.Nam}{hd.MaHd.ToString().PadLeft(4, '0')}",
                TenPhong = hd.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "Không rõ",
                TenKhachThue = hd.MaHopDongNavigation?.MaKhachNavigation?.HoTen ?? "Không rõ",
                KyHoaDon = $"{hd.Thang:D2}/{hd.Nam}",
                TongTien = hd.TongTien,
                NgayTao = hd.NgayTao.HasValue ? hd.NgayTao.Value.ToDateTime(TimeOnly.MinValue) : null,
                HanThanhToan = hd.NgayTao.HasValue ? hd.NgayTao.Value.ToDateTime(TimeOnly.MinValue).AddDays(5) : null,
                TrangThai = hd.TrangThai ?? "Chưa thanh toán"
            }).ToList();

            return View("~/Areas/QuanLy/Views/HoaDon/Index.cshtml", hoaDonList);
        }
    }
}
