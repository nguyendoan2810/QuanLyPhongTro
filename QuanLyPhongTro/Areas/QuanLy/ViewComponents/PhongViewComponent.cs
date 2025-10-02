using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using QuanLyPhongTro.Models;

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


            var phongList = await _context.Phongs
        .Join(
            _context.ChiTietPhongs,
            p => p.MaPhong,
            ctp => ctp.MaPhong,
            (p, ctp) => new
            {
                p.MaPhong, // Thêm MaPhong để dùng cho các thao tác Sửa/Xóa
                p.TenPhong,
                p.GiaPhong,
                p.TrangThai,
                ctp.DiaChi,
                ctp.DienTich,
                ctp.Tang,
                ctp.LoaiPhong
            }
         ).ToListAsync();

            ViewBag.PhongList = phongList;

            // Gọi view mặc định của ViewComponent
            return View("~/Areas/QuanLy/Views/Phong/Index.cshtml");
        }
    }
}
