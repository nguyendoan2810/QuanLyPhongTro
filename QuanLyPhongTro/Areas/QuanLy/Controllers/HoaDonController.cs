using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Areas.QuanLy.Models.ViewModels;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class HoaDonController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HoaDonController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LayDanhSachPhongDangThue()
        {
            // Lấy danh sách hợp đồng còn hiệu lực
            var hopDongsConHieuLuc = _context.HopDongs
                .Include(h => h.MaKhachNavigation)
                .Where(h => h.TrangThai == "Còn hiệu lực")
                .ToList();

            // Lấy danh sách phòng đang thuê
            var phongsDangThue = _context.Phongs
                .Include(p => p.ChiTietPhong)
                .Where(p => p.TrangThai == "Đang thuê")
                .ToList();

            // Ghép thông tin phòng với hợp đồng
            var danhSachPhong = (from p in phongsDangThue
                                 join h in hopDongsConHieuLuc on p.MaPhong equals h.MaPhong
                                 select new
                                 {
                                     MaHopDong = h.MaHopDong,
                                     MaPhong = p.MaPhong,
                                     TenPhong = p.TenPhong,
                                     DiaChi = p.ChiTietPhong?.DiaChi ?? "",
                                     GiaPhong = p.GiaPhong,
                                     TenKhach = h.MaKhachNavigation.HoTen,
                                     MaKhach = h.MaKhach
                                 }).ToList();

            return Json(danhSachPhong);
        }

        [HttpGet]
        public IActionResult LayDanhSachDichVu()
        {
            var dichVus = _context.DichVus
                .Select(d => new { d.MaDv, d.TenDv, d.DonGia })
                .ToList();

            return Json(dichVus);
        }

        [HttpGet]
        public IActionResult LayChiSoHopDong(int maHopDong)
        {
            if (maHopDong <= 0) return BadRequest("MaHopDong không hợp lệ");

            var dvIds = new[] { 1, 2 }; // 1: Điện, 2: Nước (theo cấu trúc DB của bạn)
            var result = dvIds.Select(id =>
            {
                var cs = _context.ChiSoDichVus
                    .Where(x => x.MaHopDong == maHopDong && x.MaDv == id)
                    .OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang)
                    .FirstOrDefault();

                return new
                {
                    MaDv = id,
                    ChiSoCu = cs != null ? cs.ChiSoMoi : 0, // lấy ChiSoMoi của tháng trước làm "cũ" cho tháng hiện tại
                    Thang = cs?.Thang,
                    Nam = cs?.Nam
                };
            }).ToList();

            return Json(result);
        }

        // POST: Tạo hóa đơn + chi tiết + lưu chỉ số điện/nước
        [HttpPost]
        public IActionResult TaoHoaDon([FromBody] TaoHoaDonRequest model)
        {
            if (model == null) return BadRequest(new { success = false, message = "Dữ liệu rỗng." });
            if (model.MaHopDong <= 0 || model.Thang < 1 || model.Nam < 2000)
                return BadRequest(new { success = false, message = "Dữ liệu hóa đơn không hợp lệ." });
            
            // Kiểm tra trùng hóa đơn (MaHopDong + Thang + Nam)
            var hoaDonTonTai = _context.HoaDons
                .Any(h => h.MaHopDong == model.MaHopDong
                       && h.Thang == model.Thang
                       && h.Nam == model.Nam);

            if (hoaDonTonTai)
            {
                return Conflict(new
                {
                    success = false,
                    message = "Hóa đơn cho hợp đồng này trong kỳ đã tồn tại. Không thể tạo trùng."
                });
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1) Tạo HoaDon
                var hoaDon = new HoaDon
                {
                    MaHopDong = model.MaHopDong,
                    Thang = model.Thang,
                    Nam = model.Nam,
                    NgayTao = DateOnly.FromDateTime(DateTime.Now),
                    TrangThai = "Chưa thanh toán",
                    TongTien = model.ThanhTienThangNay
                };
                _context.HoaDons.Add(hoaDon);
                _context.SaveChanges();

                // 2) Thêm chi tiết hóa đơn
                if (model.ChiTiet != null)
                {
                    foreach (var ct in model.ChiTiet)
                    {
                        if (ct.MaDv <= 0) continue;

                        var cthd = new ChiTietHoaDon
                        {
                            MaHd = hoaDon.MaHd,
                            MaDv = ct.MaDv,
                            SoLuong = ct.SoLuong,
                            DonGia = ct.DonGia,
                            ThanhTien = ct.ThanhTien
                        };
                        _context.ChiTietHoaDons.Add(cthd);
                    }
                    _context.SaveChanges();
                }

                // 3) Lưu chỉ số điện (MaDv = 1) nếu có
                if (model.ChiSoDien != null && (model.ChiSoDien.Moi > 0 || model.ChiSoDien.Cu > 0))
                {
                    bool tonTaiDien = _context.ChiSoDichVus.Any(cs =>
                         cs.MaHopDong == model.MaHopDong && cs.MaDv == 1 && cs.Thang == model.Thang && cs.Nam == model.Nam);

                    if (!tonTaiDien)
                    {
                        var csDien = new ChiSoDichVu
                        {
                            MaHopDong = model.MaHopDong,
                            MaDv = 1,
                            Thang = model.Thang,
                            Nam = model.Nam,
                            ChiSoCu = model.ChiSoDien.Cu,
                            ChiSoMoi = model.ChiSoDien.Moi
                        };
                        _context.ChiSoDichVus.Add(csDien);
                        _context.SaveChanges();
                    }
                }

                // 4) Lưu chỉ số nước (MaDv = 2) nếu có
                if (model.ChiSoNuoc != null && (model.ChiSoNuoc.Moi > 0 || model.ChiSoNuoc.Cu > 0))
                {
                    bool tonTaiNuoc = _context.ChiSoDichVus.Any(cs =>
                         cs.MaHopDong == model.MaHopDong && cs.MaDv == 2 && cs.Thang == model.Thang && cs.Nam == model.Nam);

                    if (!tonTaiNuoc)
                    {
                        var csNuoc = new ChiSoDichVu
                        {
                            MaHopDong = model.MaHopDong,
                            MaDv = 2,
                            Thang = model.Thang,
                            Nam = model.Nam,
                            ChiSoCu = model.ChiSoNuoc.Cu,
                            ChiSoMoi = model.ChiSoNuoc.Moi
                        };
                        _context.ChiSoDichVus.Add(csNuoc);
                        _context.SaveChanges();
                    }

                }

                transaction.Commit();
                return Ok(new { success = true, message = "Tạo hóa đơn thành công!", hoaDonId = hoaDon.MaHd });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new {success = false, message = "Lỗi khi tạo hóa đơn: " + (ex.InnerException?.Message ?? ex.Message)});
            }
        }

        [HttpGet]
        public IActionResult ReloadPartial()
        {
            return ViewComponent("HoaDon");
        }

        [HttpGet]
        public async Task<IActionResult> LayChiTietHoaDon(int maHd)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.ChiTietPhong)
                .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaKhachNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == maHd);

            if (hoaDon == null)
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn." });

            var phong = hoaDon.MaHopDongNavigation?.MaPhongNavigation;
            var khach = hoaDon.MaHopDongNavigation?.MaKhachNavigation;

            // Lấy chi tiết dịch vụ (không bao gồm tiền phòng)
            var chiTiet = await _context.ChiTietHoaDons
                .Include(ct => ct.MaDvNavigation)
                .Where(ct => ct.MaHd == maHd)
                .Select(ct => new
                {
                    ct.MaDv,
                    TenDv = ct.MaDvNavigation.TenDv,
                    ct.SoLuong,
                    ct.DonGia,
                    ct.ThanhTien
                })
                .ToListAsync();

            // Lấy chỉ số điện nước (nếu có)
            var chiSoDien = await _context.ChiSoDichVus
                .Where(cs => cs.MaHopDong == hoaDon.MaHopDong && cs.Thang == hoaDon.Thang && cs.Nam == hoaDon.Nam && cs.MaDv == 1)
                .Select(cs => new { cs.ChiSoCu, cs.ChiSoMoi })
                .FirstOrDefaultAsync();

            var chiSoNuoc = await _context.ChiSoDichVus
                .Where(cs => cs.MaHopDong == hoaDon.MaHopDong && cs.Thang == hoaDon.Thang && cs.Nam == hoaDon.Nam && cs.MaDv == 2)
                .Select(cs => new { cs.ChiSoCu, cs.ChiSoMoi })
                .FirstOrDefaultAsync();

            var result = new
            {
                MaHd = hoaDon.MaHd,
                TenPhong = phong != null ? $"{phong.TenPhong} - {phong.ChiTietPhong.DiaChi}" : "Không rõ",
                GiaPhong = phong?.GiaPhong ?? 0,
                TenKhachThue = khach?.HoTen ?? "Không rõ",
                Thang = hoaDon.Thang,
                Nam = hoaDon.Nam,
                TongTien = hoaDon.TongTien,
                TrangThai = hoaDon.TrangThai,
                NgayTao = hoaDon.NgayTao,
                ChiTiet = chiTiet,
                ChiSoDien = chiSoDien,
                ChiSoNuoc = chiSoNuoc
            };

            return Json(new { success = true, data = result });
        }
    }
}
