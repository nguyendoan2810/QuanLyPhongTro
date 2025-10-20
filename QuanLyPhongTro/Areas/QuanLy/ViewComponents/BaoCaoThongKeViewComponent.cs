using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class BaoCaoThongKeViewComponent : ViewComponent

    {
        private readonly QuanLyPhongTroContext _context;

        public BaoCaoThongKeViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? month, int? year)
        {

            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
                return Content("Không có quyền truy cập");

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




            var namHienTai = DateTime.Now.Year;

            // Lấy danh sách mã phòng thuộc chủ trọ
            var roomIds = _context.Phongs
                .Where(p => p.MaChuTro == maChuTro)
                .Select(p => p.MaPhong)
                .ToList();

            // Nếu không có phòng thì khỏi tính
            if (!roomIds.Any())
            {
                ViewBag.TyLeLapDayTheoThang = new List<object>();
                return View("~/Areas/QuanLy/Views/Dashboard/_TyLeLapDayChart.cshtml");
            }

            // Lấy hợp đồng thuộc các phòng đó
            var danhSachHopDong = _context.HopDongs
                .Where(hd => hd.MaPhong.HasValue && roomIds.Contains(hd.MaPhong.Value))
                .ToList();

            var ketQua = new List<object>();

            for (int thang = 1; thang <= 12; thang++)
            {
                var ngayBatDauThang = new DateTime(namHienTai, thang, 1);
                var ngayKetThucThang = ngayBatDauThang.AddMonths(1).AddDays(-1);

                int tongPhong = roomIds.Count;
                int phongDaThue = 0;

                foreach (var maPhong in roomIds)
                {
                    var hopDong = danhSachHopDong.FirstOrDefault(hd =>
                        hd.MaPhong == maPhong &&
                        hd.NgayBatDau.ToDateTime(TimeOnly.MinValue) <= ngayKetThucThang &&
                        hd.NgayKetThuc.ToDateTime(TimeOnly.MaxValue) >= ngayBatDauThang);

                    if (hopDong != null)
                        phongDaThue++;
                }

                decimal tyLe = tongPhong > 0
                    ? Math.Round((decimal)phongDaThue / tongPhong * 100, 2)
                    : 0;

                ketQua.Add(new { Thang = $"T{thang}", TyLe = tyLe });
            }

            ViewBag.TyLeLapDayTheoThang = ketQua;





            //lấy danh sách phòng trống




            var danhSachPhongTrong = (from p in _context.Phongs
                                      join ct in _context.ChiTietPhongs on p.MaPhong equals ct.MaPhong
                                      where p.MaChuTro == maChuTro && p.TrangThai == "Trống"
                                      select new
                                      {
                                          TenPhong = p.TenPhong,
                                          GiaPhong = p.GiaPhong,
                                          DiaChi = ct.DiaChi,
                                          DienTich = ct.DienTich,
                                          LoaiPhong = ct.LoaiPhong,
                                          MoTa = ct.MoTa
                                      }).ToList();

            ViewBag.DanhSachPhongTrong = danhSachPhongTrong;



            //Khách chưa thanh toán
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            var danhSachKhachChuaThanhToan = (
                from hd in _context.HoaDons
                join hop in _context.HopDongs on hd.MaHopDong equals hop.MaHopDong
                join p in _context.Phongs on hop.MaPhong equals p.MaPhong
                join kt in _context.KhachThues on hop.MaKhach equals kt.MaKhach
                where p.MaChuTro == maChuTro
                      && hd.TrangThai == "Chưa thanh toán"
                      // 🟢 Chỉ lấy hóa đơn trong tháng và năm hiện tại
                      && hd.NgayTao.Value.Month == currentMonth
                      && hd.NgayTao.Value.Year == currentYear
                select new
                {
                    HoTen = kt.HoTen,
                    TenPhong = p.TenPhong,
                    TongTien = hd.TongTien,
                    NgayTao = hd.NgayTao
                }
            )
            .AsEnumerable() // chuyển sang xử lý phía client để dùng DateOnly
            .Select(x => new
            {
                x.HoTen,
                x.TenPhong,
                x.TongTien,
                x.NgayTao,
                SoNgayChenhLech = (DateTime.Now.Date - x.NgayTao.Value.ToDateTime(TimeOnly.MinValue).Date).Days,
                TrangThai = (DateTime.Now.Date - x.NgayTao.Value.ToDateTime(TimeOnly.MinValue).Date).Days > 5
                    ? "Quá hạn"
                    : "Sắp đến hạn"
            })
            .ToList();

            ViewBag.DanhSachKhachChuaThanhToan = danhSachKhachChuaThanhToan;





            return View("~/Areas/QuanLy/Views/BaoCaoThongKe/Index.cshtml");
        }
        
    }
}
