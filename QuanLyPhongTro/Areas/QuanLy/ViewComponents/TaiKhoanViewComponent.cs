using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Areas.QuanLy.Models.ViewModels;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class TaiKhoanViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public TaiKhoanViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
            {
                return Content("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            // ====== LẤY TÀI KHOẢN CHỦ TRỌ ======
            var chuTro = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.MaChuTro == maChuTro && t.VaiTro == "Admin");

            var danhSach = new List<TaiKhoanViewModel>();

            if (chuTro != null)
            {
                danhSach.Add(new TaiKhoanViewModel
                {
                    HoTen = chuTro.HoTen ?? "(Không có tên)",
                    Email = chuTro.Email ?? "",
                    VaiTro = "Admin"
                });
            }

            // ===== LẤY CÁC KHÁCH ĐÃ CÓ TÀI KHOẢN =====
            var khachCoTaiKhoan = await _context.HopDongs
                .Include(h => h.MaKhachNavigation)
                .Include(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.ChiTietPhong)
                .Where(h => h.MaPhongNavigation.MaChuTro == maChuTro
                    && _context.TaiKhoans.Any(t => t.MaKhach == h.MaKhach && t.VaiTro == "Khach"))
                .Select(h => new TaiKhoanViewModel
                {
                    MaTk = _context.TaiKhoans.FirstOrDefault(t => t.MaKhach == h.MaKhach).MaTk,
                    HoTen = h.MaKhachNavigation.HoTen,
                    VaiTro = "Khach",
                    TenPhong = h.MaPhongNavigation.TenPhong,
                    DiaChi = h.MaPhongNavigation.ChiTietPhong != null
                        ? h.MaPhongNavigation.ChiTietPhong.DiaChi
                        : "(Chưa có địa chỉ)"
                })
                .Distinct()
                .ToListAsync();

            danhSach.AddRange(khachCoTaiKhoan);

            return View("~/Areas/QuanLy/Views/TaiKhoan/Index.cshtml", danhSach);
        }
    }
}
