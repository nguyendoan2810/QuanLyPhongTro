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
        public IActionResult Filter(string trangThai, int? thang, int? nam)
        {
            // nếu thang/nam = 0 thì coi là null (nếu model binder set 0 khi empty)
            int? th = (thang.HasValue && thang.Value > 0) ? thang : null;
            int? ny = (nam.HasValue && nam.Value > 0) ? nam : null;

            return ViewComponent("HoaDon", new { trangThai, thang = th, nam = ny });
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
            if (maHopDong <= 0) return Json("MaHopDong không hợp lệ");

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
            if (model == null) return Json(new { success = false, message = "Dữ liệu rỗng." });
            if (model.MaHopDong <= 0 || model.Thang < 1 || model.Nam < 2000)
                return Json(new { success = false, message = "Dữ liệu hóa đơn không hợp lệ." });
            
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

        [HttpPost]
        public IActionResult XoaHoaDon(int maHd)
        {
            if (maHd <= 0)
                return Json(new { success = false, message = "Mã hóa đơn không hợp lệ." });

            var hoaDon = _context.HoaDons
                .Include(h => h.MaHopDongNavigation)
                .FirstOrDefault(h => h.MaHd == maHd);

            if (hoaDon == null)
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn cần xóa." });

            if (hoaDon.TrangThai == "Đã thanh toán")
                return Json(new { success = false, message = "Không thể xóa hóa đơn đã thanh toán." });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // ✅ Kiểm tra Tháng/Năm có null không
                if (hoaDon.Thang == null || hoaDon.Nam == null)
                    return Json(new { success = false, message = "Hóa đơn thiếu thông tin tháng hoặc năm." });

                int maHopDong = hoaDon.MaHopDong ?? 0;
                int thang = hoaDon.Thang.Value;
                int nam = hoaDon.Nam.Value;

                // ✅ Xóa chi tiết hóa đơn
                var chiTiet = _context.ChiTietHoaDons
                    .Where(ct => ct.MaHd == maHd)
                    .ToList();

                if (chiTiet.Any())
                {
                    _context.ChiTietHoaDons.RemoveRange(chiTiet);
                    _context.SaveChanges();
                }

                // ✅ Xóa chỉ số điện nước thuộc hóa đơn này
                var chiSoLienQuan = _context.ChiSoDichVus
                    .Where(cs => cs.MaHopDong == maHopDong && cs.Thang == thang && cs.Nam == nam)
                    .ToList();

                if (chiSoLienQuan.Any())
                {
                    _context.ChiSoDichVus.RemoveRange(chiSoLienQuan);
                    _context.SaveChanges();
                }

                // ✅ Cuối cùng xóa hóa đơn
                _context.HoaDons.Remove(hoaDon);
                _context.SaveChanges();

                transaction.Commit();
                return Ok(new { success = true, message = "Xóa hóa đơn thành công!" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa hóa đơn: " + (ex.InnerException?.Message ?? ex.Message)
                });
            }
        }

        [HttpPost]
        public IActionResult CapNhatHoaDon([FromBody] TaoHoaDonRequest model)
        {
            if (model == null) return Json(new { success = false, message = "Dữ liệu rỗng." });
            
            if (model.MaHd <= 0) return Json(new { success = false, message = "Thiếu mã hóa đơn cần cập nhật." });
            
            if (model.Thang < 1 || model.Nam < 2000)
                return Json(new { success = false, message = "Dữ liệu hóa đơn không hợp lệ." });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var hoaDon = _context.HoaDons
                    .Include(h => h.ChiTietHoaDons)
                    .FirstOrDefault(h => h.MaHd == model.MaHd);

                if (hoaDon == null)
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn." });

                // Nếu MaHopDong null thì lỗi (an toàn hơn)
                if (hoaDon.MaHopDong == null)
                    return Json(new { success = false, message = "Hóa đơn thiếu MaHopDong." });

                // ======= KIỂM TRA TRÙNG KỲ =======
                bool trungKy = _context.HoaDons.Any(h =>
                    h.MaHopDong == hoaDon.MaHopDong &&
                    h.Thang == model.Thang &&
                    h.Nam == model.Nam &&
                    h.MaHd != hoaDon.MaHd);

                if (trungKy)
                    return Json(new { success = false, message = "Đã tồn tại hóa đơn cho hợp đồng này trong tháng/năm này." });

                // ===== Valid backend: kiểm tra chỉ số điện/nước =====
                if (model.ChiSoDien != null && model.ChiSoDien.Moi < model.ChiSoDien.Cu)
                    return Json(new { success = false, message = "Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số cũ." });

                if (model.ChiSoNuoc != null && model.ChiSoNuoc.Moi < model.ChiSoNuoc.Cu)
                    return Json(new { success = false, message = "Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số cũ." });

                // ===== Cập nhật thông tin hóa đơn =====
                hoaDon.Thang = model.Thang;
                hoaDon.Nam = model.Nam;
                hoaDon.TongTien = model.ThanhTienThangNay;
                hoaDon.TrangThai = "Chưa thanh toán";

                // ===== Cập nhật chi tiết hóa đơn =====
                _context.ChiTietHoaDons.RemoveRange(hoaDon.ChiTietHoaDons);

                if (model.ChiTiet != null)
                {
                    foreach (var ct in model.ChiTiet)
                    {
                        if (ct.MaDv <= 0) continue;

                        _context.ChiTietHoaDons.Add(new ChiTietHoaDon
                        {
                            MaHd = hoaDon.MaHd,
                            MaDv = ct.MaDv,
                            DonGia = ct.DonGia,
                            SoLuong = ct.SoLuong,
                            ThanhTien = ct.ThanhTien
                        });
                    }
                }

                // ===== Cập nhật chỉ số điện/nước =====
                var csDien = _context.ChiSoDichVus.FirstOrDefault(x =>
                    x.MaHopDong == hoaDon.MaHopDong && x.MaDv == 1 && x.Thang == model.Thang && x.Nam == model.Nam);

                var csNuoc = _context.ChiSoDichVus.FirstOrDefault(x =>
                    x.MaHopDong == hoaDon.MaHopDong && x.MaDv == 2 && x.Thang == model.Thang && x.Nam == model.Nam);

                if (model.ChiSoDien != null)
                {
                    if (csDien != null)
                    {
                        csDien.ChiSoCu = model.ChiSoDien.Cu;
                        csDien.ChiSoMoi = model.ChiSoDien.Moi;
                    }
                    else
                    {
                        _context.ChiSoDichVus.Add(new ChiSoDichVu
                        {
                            MaHopDong = hoaDon.MaHopDong ?? 0,
                            MaDv = 1,
                            Thang = model.Thang,
                            Nam = model.Nam,
                            ChiSoCu = model.ChiSoDien.Cu,
                            ChiSoMoi = model.ChiSoDien.Moi
                        });
                    }
                }

                if (model.ChiSoNuoc != null)
                {
                    if (csNuoc != null)
                    {
                        csNuoc.ChiSoCu = model.ChiSoNuoc.Cu;
                        csNuoc.ChiSoMoi = model.ChiSoNuoc.Moi;
                    }
                    else
                    {
                        _context.ChiSoDichVus.Add(new ChiSoDichVu
                        {
                            MaHopDong = hoaDon.MaHopDong ?? 0,
                            MaDv = 2,
                            Thang = model.Thang,
                            Nam = model.Nam,
                            ChiSoCu = model.ChiSoNuoc.Cu,
                            ChiSoMoi = model.ChiSoNuoc.Moi
                        });
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                return Ok(new { success = true, message = "Cập nhật hóa đơn thành công!" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật hóa đơn: " + (ex.InnerException?.Message ?? ex.Message)
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToanHoaDon(int maHd)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaPhongNavigation)
                    .ThenInclude(p => p.ChiTietPhong)
                    .FirstOrDefaultAsync(h => h.MaHd == maHd);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

                if (hoaDon.TrangThai == "Đã thanh toán")
                    return Json(new { success = false, message = "Hóa đơn này đã được thanh toán." });

                // Cập nhật trạng thái hóa đơn
                hoaDon.TrangThai = "Đã thanh toán";
                _context.HoaDons.Update(hoaDon);

                // Lấy thông tin bổ sung
                var phong = hoaDon.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "Không rõ";
                var diaChi = hoaDon.MaHopDongNavigation?.MaPhongNavigation?.ChiTietPhong?.DiaChi ?? "Không rõ";
                var ky = $"{hoaDon.Thang}/{hoaDon.Nam}";

                // Tạo bản ghi ThuChi
                var thuChi = new ThuChi
                {
                    Ngay = DateTime.Now,
                    Loai = "Thu",
                    SoTien = hoaDon.TongTien,
                    NoiDung = $"Thanh toán hóa đơn {phong} - {diaChi} kỳ {ky}",
                    MaHd = hoaDon.MaHd
                };

                _context.ThuChis.Add(thuChi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thanh toán hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thanh toán: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GuiThongBaoThanhToan(int maHd)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaKhachNavigation)
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.ChiTietPhong)
                    .FirstOrDefaultAsync(h => h.MaHd == maHd);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

                var khach = hoaDon.MaHopDongNavigation?.MaKhachNavigation;
                if (khach == null)
                    return Json(new { success = false, message = "Không tìm thấy khách thuê liên quan." });

                // Tìm tài khoản của khách thuê
                var taiKhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.MaKhach == khach.MaKhach);

                if (taiKhoan == null)
                    return Json(new { success = false, message = "Khách thuê chưa có tài khoản." });

                // Lấy thông tin phòng
                var tenPhong = hoaDon.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "Không rõ phòng";
                var diaChi = hoaDon.MaHopDongNavigation?.MaPhongNavigation?.ChiTietPhong?.DiaChi ?? "Không rõ địa chỉ";

                // Tạo thông báo
                var thongBao = new ThongBao
                {
                    MaTk = taiKhoan.MaTk,
                    NoiDung = $"Bạn có hóa đơn tháng {hoaDon.Thang}/{hoaDon.Nam} của phòng {tenPhong}-{diaChi} cần thanh toán.",
                    NgayGui = DateTime.Now,
                    Loai = "ThanhToan"
                };

                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã gửi thông báo nhắc nhở thanh toán cho khách thuê." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi gửi thông báo: " + ex.Message });
            }
        }
    }
}