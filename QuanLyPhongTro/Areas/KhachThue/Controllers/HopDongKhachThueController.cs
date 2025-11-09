using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class HopDongKhachThueController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HopDongKhachThueController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GiaHan(int MaHopDong, int SoThang)
        {
            // load hợp đồng kèm phòng và chủ trọ
            var hopDong = _context.HopDongs
                .Include(h => h.MaPhongNavigation)
                .ThenInclude(p => p.ChiTietPhong)
                .FirstOrDefault(h => h.MaHopDong == MaHopDong);

            if (hopDong == null || hopDong.MaPhongNavigation == null)
                return BadRequest();

            // lấy id chủ trọ từ phòng
            var maChuTro = hopDong.MaPhongNavigation.MaChuTro;
            if (maChuTro == null)
                return BadRequest();

            // tìm tài khoản của chủ trọ (TaiKhoan có MaChuTro = id chủ trọ)
            var chuTroTaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaChuTro == maChuTro);
            if (chuTroTaiKhoan == null)
                return BadRequest();

            // xác định ngày bắt đầu mới = ngày kế tiếp của NgayKetThuc
            // HopDong.NgayKetThuc là DateOnly
            var startDate = hopDong.NgayKetThuc.AddDays(1);

            // tạo nội dung thông báo theo yêu cầu: tên phòng + địa chỉ + số tháng + ngày bắt đầu
            var tenPhong = hopDong.MaPhongNavigation?.TenPhong ?? "không rõ";
            var diaChi = hopDong.MaPhongNavigation?.ChiTietPhong?.DiaChi ?? "không rõ";

            var noiDung = $"Yêu cầu gia hạn hợp đồng phòng {tenPhong} tại {diaChi} thêm {SoThang} tháng. Bắt đầu từ {startDate.ToString("dd/MM/yyyy")}.";

            var thongBao = new ThongBao
            {
                MaTk = chuTroTaiKhoan.MaTk, // gửi tới tài khoản chủ trọ
                NoiDung = noiDung,
                NgayGui = DateTime.Now,
                Loai = "HopDong"
            };

            _context.ThongBaos.Add(thongBao);
            _context.SaveChanges();

            return Content("OK");
        }

        [HttpPost]
        public IActionResult KetThuc(int MaHopDong, DateTime NgayKetThucThucTe)
        {
            var hopDong = _context.HopDongs
                .Include(h => h.MaPhongNavigation)
                .ThenInclude(p => p.ChiTietPhong)
                .FirstOrDefault(h => h.MaHopDong == MaHopDong);

            if (hopDong == null || hopDong.MaPhongNavigation == null)
                return BadRequest();

            // lấy mã chủ trọ từ phòng
            var maChuTro = hopDong.MaPhongNavigation.MaChuTro;
            if (maChuTro == null)
                return BadRequest();

            var chuTroTaiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.MaChuTro == maChuTro);
            if (chuTroTaiKhoan == null)
                return BadRequest();

            // tạo nội dung thông báo
            var tenPhong = hopDong.MaPhongNavigation?.TenPhong ?? "không rõ";
            var diaChi = hopDong.MaPhongNavigation?.ChiTietPhong?.DiaChi ?? "không rõ";
            var ngayKetThucThucTeStr = NgayKetThucThucTe.ToString("dd/MM/yyyy");

            var noiDung = $"Yêu cầu kết thúc hợp đồng phòng {tenPhong} tại {diaChi} vào ngày {ngayKetThucThucTeStr}.";

            var thongBao = new ThongBao
            {
                MaTk = chuTroTaiKhoan.MaTk,
                NoiDung = noiDung,
                NgayGui = DateTime.Now,
                Loai = "HopDong"
            };

            _context.ThongBaos.Add(thongBao);
            _context.SaveChanges();

            return Content("OK");
        }
    }
}