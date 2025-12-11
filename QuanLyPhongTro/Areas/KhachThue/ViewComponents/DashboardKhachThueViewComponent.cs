using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.ViewComponents
{
    public class DashboardKhachThueViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public DashboardKhachThueViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var maKhach = HttpContext.Session.GetInt32("MaKhach");
            // Nếu chưa đăng nhập, vẫn trả về model rỗng
            if (maKhach == null)
            {
                var emptyModel = new DashboardKhachThueViewModel
                {
                    TenPhong = "Chưa đăng nhập",
                    DiaChi = "",
                    TrangThaiHopDong = "Không có dữ liệu",
                    SoHoaDonChuaThanhToan = 0,
                    SoThongBaoMoi = 0,
                    DanhSachThongBao = new List<ThongBao>()
                };
                return View("~/Areas/KhachThue/Views/DashboardKhachThue/Index.cshtml", emptyModel);
            }

            // Lấy hợp đồng hiện tại (mới nhất của khách thuê)
            var hopDong = await _context.HopDongs
                .Include(h => h.MaPhongNavigation)
                .ThenInclude(p => p.ChiTietPhong)
                .Where(h => h.MaKhach == maKhach)
                .OrderByDescending(h => h.NgayBatDau)
                .FirstOrDefaultAsync();

            string tenPhong = "Chưa có";
            string diaChi = "Chưa xác định";
            string trangThaiHopDong = "Không có hợp đồng";

            if (hopDong != null)
            {
                tenPhong = hopDong.MaPhongNavigation?.TenPhong ?? "Không rõ";
                diaChi = hopDong.MaPhongNavigation?.ChiTietPhong?.DiaChi ?? "Không rõ";
                trangThaiHopDong = hopDong.TrangThai == "Đã kết thúc" ? "Đã kết thúc" : "Còn hiệu lực";
            }

            // Số hóa đơn chưa thanh toán
            int soHoaDonChuaTT = 0;
            if (hopDong != null)
            {
                soHoaDonChuaTT = await _context.HoaDons
                    .Where(h => h.MaHopDong == hopDong.MaHopDong && h.TrangThai == "Chưa thanh toán")
                    .CountAsync();
            }

            // Số thông báo mới trong 5 ngày gần nhất
            var maTk = HttpContext.Session.GetInt32("MaTk");
            int soThongBaoMoi = 0;
            List<ThongBao> danhSachThongBao = new();

            if (maTk != null)
            {
                DateTime tuNgay = DateTime.Now.AddDays(-5);
                danhSachThongBao = await _context.ThongBaos
                    .Where(tb => tb.MaTk == maTk && tb.NgayGui >= tuNgay)
                    .OrderByDescending(tb => tb.NgayGui)
                    .ToListAsync();

                soThongBaoMoi = danhSachThongBao.Count;
            }

            // Tạo view model tóm tắt
            var model = new DashboardKhachThueViewModel
            {
                TenPhong = tenPhong,
                DiaChi = diaChi,
                TrangThaiHopDong = trangThaiHopDong,
                SoHoaDonChuaThanhToan = soHoaDonChuaTT,
                SoThongBaoMoi = soThongBaoMoi,
                DanhSachThongBao = danhSachThongBao
            };

            return View("~/Areas/KhachThue/Views/DashboardKhachThue/Index.cshtml", model);
        }
    }

    public class DashboardKhachThueViewModel
    {
        public string TenPhong { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string TrangThaiHopDong { get; set; } = "";
        public int SoHoaDonChuaThanhToan { get; set; }
        public int SoThongBaoMoi { get; set; }
        public List<ThongBao> DanhSachThongBao { get; set; } = new();
    }
}