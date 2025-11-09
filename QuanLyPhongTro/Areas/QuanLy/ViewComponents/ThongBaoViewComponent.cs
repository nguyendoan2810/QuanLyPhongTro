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

        public async Task<IViewComponentResult> InvokeAsync(string loc = "all")

        {

            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            var maTkChuTro = HttpContext.Session.GetInt32("MaTk"); // 🔹 mã tài khoản chủ trọ đăng nhập

            if (maChuTro == null)
            {
                return Content("Không có quyền truy cập");
            }

            // 🔹 Lấy danh sách phòng chủ trọ đang quản lý
            var danhSachPhong = (
                from p in _context.Phongs
                where p.MaChuTro == maChuTro
                select new
                {
                    p.MaPhong,
                    p.TenPhong,
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

            // 🔹 Lấy danh sách thông báo (cả khách thuê và chủ trọ)
            DateTime thoiGianGioiHan = DateTime.Now.AddDays(-5);

            var danhSachThongBao = (
                from tb in _context.ThongBaos
                join tk in _context.TaiKhoans on tb.MaTk equals tk.MaTk
                join kt in _context.KhachThues on tk.MaKhach equals kt.MaKhach into khachGroup
                from kt in khachGroup.DefaultIfEmpty()
                join hd in _context.HopDongs on kt.MaKhach equals hd.MaKhach into hopDongGroup
                from hd in hopDongGroup.DefaultIfEmpty()
                join p in _context.Phongs on hd.MaPhong equals p.MaPhong into phongGroup
                from p in phongGroup.DefaultIfEmpty()
                where tb.NgayGui >= thoiGianGioiHan
               &&
               (
                   // Khách thuê mà chủ trọ đang quản lý
                   (kt != null && p != null && p.MaChuTro == maChuTro && hd.TrangThai == "Còn hiệu lực")
                   ||
                   // Thông báo do chính chủ trọ gửi
                   (tb.MaTk == maTkChuTro)
               )
                orderby tb.NgayGui descending
                select new
                {
                    tb.MaTb,
                    tb.NoiDung,
                    tb.NgayGui,
                    tb.Loai,
                    NguoiNhan = kt != null ? kt.HoTen : "Chủ trọ",
                    TenPhong = p != null ? p.TenPhong : "Hệ thống"
                }
            ).ToList().GroupBy(tb => tb.MaTb).Select(g => g.First()).Take(30).ToList();
            var danhSachThongBaoGoc = danhSachThongBao.ToList(); // lưu danh sách gốc

            if (loc == "chu")
            {
                danhSachThongBao = danhSachThongBaoGoc
                    .Where(tb => tb.NguoiNhan == "Chủ trọ")
                    .ToList();
            }
            else if (loc == "khach")
            {
                danhSachThongBao = danhSachThongBaoGoc
                    .Where(tb => tb.NguoiNhan != "Chủ trọ")
                    .ToList();
            }
            else
            {
                danhSachThongBao = danhSachThongBaoGoc;
            }


            ViewBag.DanhSachThongBao = danhSachThongBao;
            ViewBag.TongThongBao = danhSachThongBao.Count;

            return View("~/Areas/QuanLy/Views/ThongBao/Index.cshtml");
        }


    }
}
