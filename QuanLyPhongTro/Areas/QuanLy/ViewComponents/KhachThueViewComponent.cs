using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class KhachThueViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public KhachThueViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var KhachThueList = await _context.KhachThues
                .Join(
                    _context.HopDongs, // Tên bảng hợp đồng trong DbContext
                    kt => kt.MaKhach, // Khóa chung từ bảng KhachThue
                    hd => hd.MaKhach, // Khóa chung từ bảng HopDong
                    (kt, hd) => new // Tạo đối tượng ẩn danh mới
                    {
                        // Chọn các thuộc tính bạn muốn hiển thị
                        kt.MaKhach,
                        kt.HoTen,
                        kt.SoDienThoai,
                        kt.DiaChi,
                        hd.NgayBatDau,
                        hd.NgayKetThuc,
                        hd.TrangThai,
                        hd.MaPhong
                    }
                ).ToListAsync();

            ViewBag.KhachThueList = KhachThueList;

            // Gọi view của khách thuê, đảm bảo view này tồn tại
            return View("~/Areas/QuanLy/Views/KhachThue/Index.cshtml");
        }
    }
}