using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class DashboardViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public DashboardViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? month, int? year)
        {
            // Lấy mã chủ trọ từ session
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null) return Content("Không có quyền truy cập");

            // Tổng số phòng và phòng đã thuê
            var soLuongPhong = _context.Phongs.Count(p => p.MaChuTro == maChuTro);
            var soPhongDaThue = _context.Phongs.Count(p => p.MaChuTro == maChuTro && p.TrangThai == "Đang thuê");

            ViewBag.SoLuongPhong = soLuongPhong;
            ViewBag.SoPhongDaThue = soPhongDaThue;

            // Thu chi theo tháng
            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;

            //Lay danh sach thu chi
            var thuChiList = _context.ThuChis
                .Where(tc => tc.Ngay.HasValue
                          && tc.Ngay.Value.Month == currentMonth
                          && tc.Ngay.Value.Year == currentYear)
                .OrderByDescending(tc => tc.Ngay).ToList();

            //tong thu / tong chi / loi nhuan
            var tongThu = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "thu").Sum(tc => tc.SoTien);

            var tongChi = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "chi").Sum(tc => tc.SoTien);

            var loiNhuan = tongThu - tongChi;

            //truyen sang view
            ViewBag.TongThu = tongThu;
            ViewBag.TongChi = tongChi;
            ViewBag.LoiNhuan = loiNhuan;

            ViewBag.ThuList = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "thu").OrderByDescending(x => x.Ngay).ToList();
            ViewBag.ChiList = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "chi").OrderByDescending(x => x.Ngay).ToList();

            ViewBag.SelectedMonth = currentMonth;
            ViewBag.SelectedYear = currentYear;

            // Tong khach thue
            var tongKhachThue = (from hd in _context.HopDongs
                                 join p in _context.Phongs on hd.MaPhong equals p.MaPhong
                                 where p.MaChuTro == maChuTro
                                 select hd.MaKhach).Distinct().Count();

            ViewBag.TongKhachThue = tongKhachThue;



            // Chart doanh thu theo thang
            var doanhThuThang = _context.ThuChis
                .Where(x => x.Loai.ToLower() == "thu" && x.Ngay.HasValue)
                .GroupBy(x => x.Ngay.Value.Month)
                .Select(g => new {
                     Thang = g.Key,
                     TongDoanhThu = g.Sum(x => x.SoTien)
                 })
                .OrderBy(x => x.Thang)
                .ToList();

            ViewBag.DoanhThuThang = doanhThuThang;

            // Tỷ lệ phòng
            var phongTheoTrangThai = _context.Phongs
               .Where(p => p.MaChuTro == maChuTro)
               .GroupBy(p => p.TrangThai)
               .Select(g => new {
                   TrangThai = g.Key,
                   SoLuong = g.Count()
               })
               .ToList();

            ViewBag.PhongTheoTrangThai = phongTheoTrangThai;



       
            // Tính % số phòng đã thuê của chủ trọ hiện tại
            var tongSoPhong1 = _context.Phongs.Count(p => p.MaChuTro == maChuTro);
            var soPhongDaThue1 = _context.Phongs.Count(p => p.MaChuTro == maChuTro && p.TrangThai == "Đang thuê");

            decimal tyLeLapDay = tongSoPhong1 > 0
                ? Math.Round((decimal)soPhongDaThue1 / tongSoPhong1 * 100, 2)
                : 0;

            ViewBag.SoLuongPhong = tongSoPhong1;
            ViewBag.SoPhongDaThue = soPhongDaThue1;
            ViewBag.TyLeLapDay = tyLeLapDay;




            var now = DateTime.Now;
            int thangHienTai = now.Month;
            int namHienTai = now.Year;

            int thangTruoc = thangHienTai == 1 ? 12 : thangHienTai - 1;
            int namTruoc = thangHienTai == 1 ? namHienTai - 1 : namHienTai;

            // Doanh thu tháng này
            var doanhThuThangNay = _context.ThuChis
                .Where(x => x.Loai != null
                         && x.Loai.ToLower() == "thu"
                         && x.Ngay.HasValue
                         && x.Ngay.Value.Month == thangHienTai
                         && x.Ngay.Value.Year == namHienTai)
                .Sum(x => (decimal?)x.SoTien) ?? 0;

            // Doanh thu tháng trước
            var doanhThuThangTruoc = _context.ThuChis
                .Where(x => x.Loai != null
                         && x.Loai.ToLower() == "thu"
                         && x.Ngay.HasValue
                         && x.Ngay.Value.Month == thangTruoc
                         && x.Ngay.Value.Year == namTruoc)
                .Sum(x => (decimal?)x.SoTien) ?? 0;

            // Tính tỷ lệ tăng trưởng
            decimal tyLeTangTruong = doanhThuThangTruoc > 0
                ? Math.Round((doanhThuThangNay - doanhThuThangTruoc) / doanhThuThangTruoc * 100, 2)
                : 100;

            ViewBag.TongThuThangNay = doanhThuThangNay;
            ViewBag.TongThuThangTruoc = doanhThuThangTruoc;
            ViewBag.TyLeTangTruong = tyLeTangTruong;

            return View("~/Areas/QuanLy/Views/Dashboard/Index.cshtml");
        }
    }
}