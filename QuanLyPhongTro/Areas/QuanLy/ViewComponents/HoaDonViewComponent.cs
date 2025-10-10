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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var hoaDons = _context.HoaDons
                .Include(hd => hd.MaHopDongNavigation)
                .ThenInclude(hd => hd.MaPhongNavigation)
                .Include(hd => hd.MaHopDongNavigation)
                .ThenInclude(hd => hd.MaKhachNavigation).ToList();

            var hoaDonList = hoaDons.Select(hd => new HoaDonViewModel 
            { 
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
