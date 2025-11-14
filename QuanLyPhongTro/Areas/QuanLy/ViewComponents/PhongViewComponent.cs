using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using QuanLyPhongTro.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class PhongViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public PhongViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(string trangThai = "")
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
            {
                ViewBag.PhongList = new List<object>();
                return View("~/Areas/QuanLy/Views/Phong/Index.cshtml");
            }

            var query = _context.Phongs
                .Where(p => p.MaChuTro == maChuTro)
                .Join(_context.ChiTietPhongs,
                      p => p.MaPhong,
                      ctp => ctp.MaPhong,
                      (p, ctp) => new { p, ctp })
                .GroupJoin(_context.HopDongs,
                       pc => pc.p.MaPhong,
                       hd => hd.MaPhong,
                       (pc, hdGroup) => new { pc, hdGroup })
                .SelectMany(
                     x => x.hdGroup.OrderByDescending(h => h.NgayBatDau).Take(1).DefaultIfEmpty(),
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

                          // CHỈ LẤY KHÁCH KHI PHÒNG ĐANG THUÊ
                          HoTenKhach = x.pc.p.TrangThai == "Đang thuê" && hd != null
                              ? _context.KhachThues
                                  .Where(k => k.MaKhach == hd.MaKhach)
                                  .Select(k => k.HoTen)
                                  .FirstOrDefault()
                              : "Chưa có khách thuê"
                     }
                );

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            var phongList = await query.ToListAsync();

            ViewBag.PhongList = phongList;
            return View("~/Areas/QuanLy/Views/Phong/Index.cshtml");
        }
    }
}
