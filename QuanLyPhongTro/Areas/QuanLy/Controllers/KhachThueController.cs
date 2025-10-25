using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class KhachThueController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public KhachThueController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        // Action Index gốc của bạn, nó chỉ trả về View
        // View này sau đó sẽ được ViewComponent của bạn xử lý (theo cách bạn đã setup)
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ReloadKhachThueList(string searchString) // Thêm tham số
        {
            // Trả về ViewComponent và truyền tham số searchString qua
            return ViewComponent("KhachThue", new { searchString = searchString });
        }


        // POST: /QuanLy/KhachThue/Create
        // Action này xử lý việc Thêm mới Khách thuê từ modal
        // 🟢 POST: /QuanLy/KhachThue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,Cccd,SoDienThoai,DiaChi,NgaySinh")] QuanLyPhongTro.Models.KhachThue khachThue)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Dữ liệu không hợp lệ.", errors });
            }

            try
            {
                _context.KhachThues.Add(khachThue);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm khách thuê thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm khách thuê." });
            }
        }


        // 🟡 POST: /QuanLy/KhachThue/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("MaKhach,HoTen,Cccd,SoDienThoai,DiaChi,NgaySinh")] QuanLyPhongTro.Models.KhachThue khachThue)
        {
            if (khachThue == null || khachThue.MaKhach == 0)
                return Json(new { success = false, message = "Thiếu thông tin khách thuê." });

            var existing = await _context.KhachThues.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaKhach == khachThue.MaKhach);

            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy khách thuê." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Dữ liệu không hợp lệ.", errors });
            }

            try
            {
                _context.Update(khachThue);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật khách thuê thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật khách thuê." });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ID" });
            }

            // Đây là truy vấn phức tạp giống như trong ViewComponent của bạn
            // để lấy đầy đủ thông tin hiển thị lên modal "Xem"
            var khachThueDetails = await _context.KhachThues
                .Where(kt => kt.MaKhach == id)
                .Join(_context.HopDongs,
                      kt => kt.MaKhach,
                      hd => hd.MaKhach,
                      (kt, hd) => new { kt, hd })
                .Join(_context.Phongs,
                      temp => temp.hd.MaPhong,
                      p => p.MaPhong,
                      (temp, p) => new { temp.kt, temp.hd, p })
                .GroupJoin(_context.ChiTietPhongs,
                           tmp => tmp.p.MaPhong,
                           ct => ct.MaPhong,
                           (tmp, cts) => new { tmp.kt, tmp.hd, tmp.p, cts })
                .SelectMany(x => x.cts.DefaultIfEmpty(), (x, ct) => new
                {
                    MaKhach = x.kt.MaKhach,
                    HoTen = x.kt.HoTen,
                    Cccd = x.kt.Cccd,
                    SoDienThoai = x.kt.SoDienThoai,
                    DiaChi = x.kt.DiaChi,
                    NgaySinh = x.kt.NgaySinh.HasValue ? x.kt.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
                    TenPhong = x.p.TenPhong,
                    DiaChiPhong = ct != null ? ct.DiaChi : "N/A",
                    NgayBatDau = x.hd.NgayBatDau.ToString("dd/MM/yyyy"),
                    NgayKetThuc = x.hd.NgayKetThuc.ToString("dd/MM/yyyy"),
                    TienCoc = x.hd.TienCoc,
                    TrangThaiHopDong = x.hd.TrangThai
                })
                .FirstOrDefaultAsync(); // Lấy 1 bản ghi duy nhất

            if (khachThueDetails == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách thuê." });
            }

            return Json(new { success = true, data = khachThueDetails });
        }

        // GET: /QuanLy/KhachThue/Edit/5
        // Lấy thông tin khách thuê cơ bản để điền vào form "Sửa"
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ID" });
            }

            var khachThue = await _context.KhachThues.FindAsync(id);

            if (khachThue == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách thuê." });
            }

            // Trả về dữ liệu gốc, bao gồm cả NgaySinh dạng YYYY-MM-DD để input[type=date] có thể nhận
            return Json(new
            {
                success = true,
                data = new
                {
                    maKhach = khachThue.MaKhach,
                    hoTen = khachThue.HoTen,
                    cccd = khachThue.Cccd,
                    soDienThoai = khachThue.SoDienThoai,
                    diaChi = khachThue.DiaChi,
                    ngaySinh = khachThue.NgaySinh.HasValue ? khachThue.NgaySinh.Value.ToString("yyyy-MM-dd") : ""
                }
            });
        }

        // POST: /QuanLy/KhachThue/Edit/5
        // Xử lý cập nhật thông tin khách thuê


        // POST: /QuanLy/KhachThue/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var khachThue = await _context.KhachThues.FindAsync(id);
            if (khachThue == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách thuê." });
            }

            try
            {
                // Cần kiểm tra ràng buộc khóa ngoại, ví dụ: khách còn trong hợp đồng không?
                var hopDong = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaKhach == id && h.TrangThai == "Active"); // Giả sử "Active" là còn hạn
                if (hopDong != null)
                {
                    return Json(new { success = false, message = "Không thể xóa. Khách thuê này vẫn còn hợp đồng đang hoạt động." });
                }

                _context.KhachThues.Remove(khachThue);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa khách thuê thành công." });
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có ràng buộc khóa ngoại
                return Json(new { success = false, message = "Đã xảy ra lỗi. Khách thuê này có thể đang được liên kết với dữ liệu khác (Hóa đơn, Tài khoản...)." });
            }
        }
    }
}