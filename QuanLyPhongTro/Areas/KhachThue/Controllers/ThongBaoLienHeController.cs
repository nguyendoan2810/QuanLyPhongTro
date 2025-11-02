using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class ThongBaoLienHeController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public ThongBaoLienHeController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GuiYeuCau(string noiDung, string loai)
        {
            var maKhach = HttpContext.Session.GetInt32("MaKhach");
            if (maKhach == null)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại!" });

            if (string.IsNullOrWhiteSpace(noiDung))
                return Json(new { success = false, message = "Vui lòng nhập nội dung yêu cầu!" });

            // 🔹 Lấy thông tin hợp đồng để biết chủ trọ
            var hopDong = await _context.HopDongs
                .Include(h => h.MaPhongNavigation)
                .ThenInclude(p => p.ChiTietPhong)
                .Where(h => h.MaKhach == maKhach && h.TrangThai != "Đã kết thúc")
                .OrderByDescending(h => h.NgayBatDau)
                .FirstOrDefaultAsync();

            if (hopDong == null || hopDong.MaPhongNavigation == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin phòng trọ của bạn!" });

            // 🔹 Lấy mã tài khoản của chủ trọ
            var chuTro = await _context.ChuTros
                .Include(c => c.TaiKhoans)
                .FirstOrDefaultAsync(c => c.MaChuTro == hopDong.MaPhongNavigation.MaChuTro);

            if (chuTro == null)
                return Json(new { success = false, message = "Không tìm thấy chủ trọ để gửi thông báo!" });

            var maTkChuTro = chuTro.TaiKhoans.FirstOrDefault()?.MaTk;
            if (maTkChuTro == null)
                return Json(new { success = false, message = "Tài khoản chủ trọ không hợp lệ!" });

            // 🔹 Ghép nội dung: "Tên phòng - Địa chỉ: Nội dung"
            string tenPhong = hopDong.MaPhongNavigation.TenPhong ?? "Không rõ";
            string diaChi = hopDong.MaPhongNavigation.ChiTietPhong?.DiaChi ?? "Chưa có địa chỉ";
            string noiDungDayDu = $"{tenPhong} - {diaChi}: {noiDung.Trim()}";

            // 🔹 Lưu thông báo
            var thongBao = new ThongBao
            {
                MaTk = maTkChuTro.Value, // 🔸 Gửi cho chủ trọ
                NoiDung = noiDungDayDu,
                NgayGui = DateTime.Now,
                Loai = string.IsNullOrEmpty(loai) ? "Khac" : loai
            };

            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Gửi yêu cầu thành công!" });
        }
    }
}