using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class PhongController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public PhongController(QuanLyPhongTroContext context)
        {
            _context = context;
        }


        // API để lấy danh sách phòng trống
        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms()
        {
            try
            {
                var availableRooms = await _context.Phongs
                    .Where(p => p.TrangThai.ToLower() == "trống")
                    .Include(p => p.ChiTietPhong)
                    .Select(p => new
                    {
                        maPhong = p.MaPhong,
                        tenPhong = p.TenPhong,
                        giaPhong = p.GiaPhong,
                        trangThai = p.TrangThai,
                        dienTich = p.ChiTietPhong != null
                            ? p.ChiTietPhong.DienTich.ToString()
                            : "Chưa cập nhật",

                        loaiPhong = p.ChiTietPhong != null ? p.ChiTietPhong.LoaiPhong : "Chưa phân loại"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = availableRooms });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải danh sách phòng: " + ex.Message });
            }
        }


        // Hiển thị danh sách phòng theo mã chủ trọ đang đăng nhập
        public async Task<IActionResult> Index()
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
            }

            // JOIN Phong + ChiTietPhong + HopDong + KhachThue
            var phongList = await (
                from p in _context.Phongs
                join ctp in _context.ChiTietPhongs on p.MaPhong equals ctp.MaPhong
                join hd in _context.HopDongs on p.MaPhong equals hd.MaPhong into hopGroup
                from hop in hopGroup.OrderByDescending(x => x.NgayBatDau).Take(1).DefaultIfEmpty() // lấy hợp đồng mới nhất (nếu có)
                join kt in _context.KhachThues on hop.MaKhach equals kt.MaKhach into khachGroup
                from khach in khachGroup.DefaultIfEmpty()
                where p.MaChuTro == maChuTro
                select new
                {
                    p.MaPhong,
                    p.TenPhong,
                    p.GiaPhong,
                    p.TrangThai,
                    ctp.Tang,
                    ctp.DienTich,
                    ctp.LoaiPhong,
                    ctp.DiaChi,
                    HoTenKhach = khach != null ? khach.HoTen : "Chưa có khách thuê"
                }
            ).ToListAsync();

            ViewBag.PhongList = phongList;
            return View();
        }

        [HttpGet]
        public IActionResult ReloadFilterPartial(string trangThai = "")
        {
            return ViewComponent("Phong", new { trangThai });
        }

        [HttpGet]
        public IActionResult ReloadPartial()
        {
            return ViewComponent("Phong");
        }


        // Thêm phòng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phong phong, ChiTietPhong chiTiet)
        {
            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập lại!" });

            if (ModelState.IsValid)
            {
                phong.MaChuTro = maChuTro.Value;

                _context.Phongs.Add(phong);
                await _context.SaveChangesAsync();

                chiTiet.MaPhong = phong.MaPhong;
                _context.ChiTietPhongs.Add(chiTiet);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm phòng thành công!" });
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
        }

        // Lấy dữ liệu phòng để sửa
        [HttpGet]
        public async Task<IActionResult> GetPhong(int id)
        {
            var phong = await (
                from p in _context.Phongs
                join ctp in _context.ChiTietPhongs on p.MaPhong equals ctp.MaPhong
                where p.MaPhong == id
                select new
                {
                    p.MaPhong,
                    p.TenPhong,
                    p.GiaPhong,
                    p.TrangThai,
                    ctp.Tang,
                    ctp.DienTich,
                    ctp.LoaiPhong,
                    ctp.DiaChi,
                    ctp.MoTa
                }
            ).FirstOrDefaultAsync();

            if (phong == null)
                return Json(new { success = false, message = "Không tìm thấy phòng." });

            return Json(new { success = true, data = phong });
        }

        // Cập nhật phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Phong phong, ChiTietPhong chiTiet)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            var existingPhong = await _context.Phongs.FindAsync(phong.MaPhong);
            if (existingPhong == null)
                return Json(new { success = false, message = "Không tìm thấy phòng!" });

            existingPhong.TenPhong = phong.TenPhong;
            existingPhong.GiaPhong = phong.GiaPhong;
            existingPhong.TrangThai = phong.TrangThai;

            var existingChiTiet = await _context.ChiTietPhongs
                .FirstOrDefaultAsync(x => x.MaPhong == phong.MaPhong);

            if (existingChiTiet != null)
            {
                existingChiTiet.Tang = chiTiet.Tang;
                existingChiTiet.DienTich = chiTiet.DienTich;
                existingChiTiet.LoaiPhong = chiTiet.LoaiPhong;
                existingChiTiet.DiaChi = chiTiet.DiaChi;
                existingChiTiet.MoTa = chiTiet.MoTa;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // Xóa phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null)
                return Json(new { success = false, message = "Phòng không tồn tại!" });

            var chiTiet = await _context.ChiTietPhongs.FirstOrDefaultAsync(x => x.MaPhong == id);
            if (chiTiet != null)
                _context.ChiTietPhongs.Remove(chiTiet);

            _context.Phongs.Remove(phong);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa phòng thành công!" });
        }
    }
}