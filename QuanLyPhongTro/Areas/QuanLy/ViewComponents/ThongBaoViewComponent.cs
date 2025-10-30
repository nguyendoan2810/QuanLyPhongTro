using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class ThongBaoViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public ThongBaoViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
            {
                return Content("Không có quyền truy cập");
            }

            // 🔹 Lấy danh sách phòng mà chủ trọ này quản lý
            var danhSachPhong = (
                from p in _context.Phongs
                where p.MaChuTro == maChuTro
                select new
                {
                    p.MaPhong,
                    p.TenPhong,

                    // 🔹 Lấy khách thuê đang có hợp đồng còn hiệu lực trong phòng
                    KhachThues = (
                        from hd in _context.HopDongs
                        join kt in _context.KhachThues on hd.MaKhach equals kt.MaKhach
                        where hd.MaPhong == p.MaPhong && hd.TrangThai == "Còn hiệu lực"
                        select new
                        {
                            kt.MaKhach,
                            kt.HoTen,
                            hd.NgayBatDau,
                            hd.NgayKetThuc
                        }
                    ).ToList()
                }
            ).ToList();

            ViewBag.DanhSachPhong = danhSachPhong;

            // 🔹 Lấy danh sách thông báo 
            DateTime thoiGianGioiHan = DateTime.Now.AddDays(-5);
            var danhSachThongBao = (
               from tb in _context.ThongBaos
               join tk in _context.TaiKhoans on tb.MaTk equals tk.MaTk
               join kt in _context.KhachThues on tk.MaKhach equals kt.MaKhach
               join hd in _context.HopDongs on kt.MaKhach equals hd.MaKhach
               join p in _context.Phongs on hd.MaPhong equals p.MaPhong
               where p.MaChuTro == maChuTro
                     && hd.TrangThai == "Còn hiệu lực"
                     && tb.NgayGui >= thoiGianGioiHan // 🔸 Chỉ lấy thông báo trong 5 ngày gần đây
               orderby tb.NgayGui descending
               select new
               {
                   tb.MaTb,
                   tb.NoiDung,
                   tb.NgayGui,
                   tb.Loai,
                   NguoiNhan = kt.HoTen,
                   TenPhong = p.TenPhong
               }
           ).ToList()
            .GroupBy(tb => tb.MaTb)
            .Select(g => g.First())
            .Take(30)
            .ToList();

            ViewBag.DanhSachThongBao = danhSachThongBao;
            ViewBag.TongThongBao = danhSachThongBao.Count;

            return View("~/Areas/QuanLy/Views/ThongBao/Index.cshtml");
        }

    }
}
