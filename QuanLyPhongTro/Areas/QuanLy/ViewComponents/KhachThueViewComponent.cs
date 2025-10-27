using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using QuanLyPhongTro.Models;
using Microsoft.AspNetCore.Http; // Thêm
using System.Collections.Generic; // Thêm

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class KhachThueViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public KhachThueViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        // Thêm tham số searchString
        public async Task<IViewComponentResult> InvokeAsync(string searchString)
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
            {
                ViewBag.KhachThueList = new List<object>();
                return View("~/Areas/QuanLy/Views/KhachThue/Index.cshtml", new List<object>());
            }

            var query =
                from kt in _context.KhachThues
                join hd in _context.HopDongs on kt.MaKhach equals hd.MaKhach
                join p in _context.Phongs on hd.MaPhong equals p.MaPhong
                where p.MaChuTro == maChuTro
                join ctp in _context.ChiTietPhongs on p.MaPhong equals ctp.MaPhong into ctpGroup
                from ct in ctpGroup.DefaultIfEmpty()
                select new { kt, hd, p, ct };

            if (!string.IsNullOrEmpty(searchString))
            {
                string lower = searchString.ToLower();
                query = query.Where(x =>
                    x.kt.HoTen.ToLower().Contains(lower) ||
                    x.kt.Cccd.ToLower().Contains(lower) ||
                    x.kt.SoDienThoai.ToLower().Contains(lower)
                );
            }

            var khachThueList = await query
                .Select(x => new
                {
                    MaKhach = x.kt.MaKhach,
                    HoTen = x.kt.HoTen,
                    SoDienThoai = x.kt.SoDienThoai,
                    DiaChi = x.kt.DiaChi,
                    TenPhong = x.p.TenPhong,
                    DiaChiPhong = x.ct != null ? x.ct.DiaChi : "N/A",
                    NgayBatDau = x.hd.NgayBatDau.ToString("dd/MM/yyyy"),
                    NgayKetThuc = x.hd.NgayKetThuc.ToString("dd/MM/yyyy")
                })
                .Distinct()
                .ToListAsync();

            ViewBag.KhachThueList = khachThueList;

            // ⚡ Quan trọng: tắt layout để chỉ trả phần HTML trong Index.cshtml
            ViewData["Layout"] = null;

            return View("~/Areas/QuanLy/Views/KhachThue/Index.cshtml");
        }

    }
}