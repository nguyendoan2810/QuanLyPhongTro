using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using QuanLyPhongTro.Models;
using Microsoft.AspNetCore.Http; // <-- Thêm thư viện này để dùng Session
using System.Collections.Generic; // <-- Thêm thư viện này để dùng List

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class PhongViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public PhongViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // ==================================================
            // === BƯỚC 1: LẤY MÃ CHỦ TRỌ TỪ SESSION ===
            // ==================================================
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");

            // Nếu không có session, trả về 1 danh sách rỗng
            if (maChuTro == null)
            {
                ViewBag.PhongList = new List<object>(); // Tạo danh sách rỗng
                return View("~/Areas/QuanLy/Views/Phong/Index.cshtml");
            }

            // ==================================================
            // === BƯỚC 2: THÊM .Where() ĐỂ LỌC THEO CHỦ TRỌ ===
            // ==================================================
            var phongList = await _context.Phongs

                // ===== THÊM DÒNG NÀY ĐẦU TIÊN =====
                .Where(p => p.MaChuTro == maChuTro)
                // ===================================

                .Join(
                    _context.ChiTietPhongs,
                    p => p.MaPhong,
                    ctp => ctp.MaPhong,
                    (p, ctp) => new { p, ctp }
                )
                .GroupJoin(
                    _context.HopDongs,
                    pc => pc.p.MaPhong,
                    hd => hd.MaPhong,
                    (pc, hdGroup) => new { pc, hdGroup }
                )
                .SelectMany(
                    x => x.hdGroup.OrderByDescending(h => h.NgayBatDau).Take(1).DefaultIfEmpty(), // Lấy HĐ mới nhất
                    (x, hd) => new
                    {
                        x.pc.p.MaPhong,
                        x.pc.p.TenPhong,
                        x.pc.p.GiaPhong,
                        x.pc.p.TrangThai,
                        x.pc.ctp.DiaChi,
                        x.pc.ctp.DienTich,
                        x.pc.ctp.Tang,
                        x.pc.ctp.LoaiPhong,
                        x.pc.ctp.MoTa,
                        HoTenKhach = (hd != null) ? _context.KhachThues
                            .Where(k => k.MaKhach == hd.MaKhach)
                            .Select(k => k.HoTen)
                            .FirstOrDefault() : "Chưa có khách thuê" // Sửa lại logic lấy tên
                    }
                )
                .ToListAsync();

            ViewBag.PhongList = phongList;

            return View("~/Areas/QuanLy/Views/Phong/Index.cshtml");
        }
    }
}