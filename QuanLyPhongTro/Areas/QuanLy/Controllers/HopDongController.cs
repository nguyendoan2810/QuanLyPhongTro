using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class HopDongController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public HopDongController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API lấy tất cả hợp đồng
        [HttpGet]
        public async Task<IActionResult> GetAllContracts()
        {
            try
            {
                var contracts = await _context.HopDongs
                    .Include(h => h.MaKhachNavigation)
                    .Include(h => h.MaPhongNavigation)
                    .Select(h => new
                    {
                        MaHopDong = h.MaHopDong,
                        TenKhach = h.MaKhachNavigation != null ? h.MaKhachNavigation.HoTen : "Chưa xác định",
                        TenPhong = h.MaPhongNavigation != null ? h.MaPhongNavigation.TenPhong : "Chưa xác định",
                        NgayBatDau = h.NgayBatDau.ToString("dd/MM/yyyy"),
                        NgayKetThuc = h.NgayKetThuc.ToString("dd/MM/yyyy"),
                        GiaPhong = h.MaPhongNavigation != null ? h.MaPhongNavigation.GiaPhong : 0,
                        TienCoc = h.TienCoc ?? 0,
                        TrangThai = h.TrangThai ?? "Chưa xác định"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = contracts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải danh sách hợp đồng: " + ex.Message });
            }
        }

        // API lọc hợp đồng còn hiệu lực
        [HttpGet]
        public async Task<IActionResult> FilterActiveContracts()
        {
            try
            {
                var contracts = await _context.HopDongs
                    .Include(h => h.MaKhachNavigation)
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => h.TrangThai == "Còn hiệu lực")
                    .Select(h => new
                    {
                        MaHopDong = h.MaHopDong,
                        TenKhach = h.MaKhachNavigation != null ? h.MaKhachNavigation.HoTen : "Chưa xác định",
                        TenPhong = h.MaPhongNavigation != null ? h.MaPhongNavigation.TenPhong : "Chưa xác định",
                        NgayBatDau = h.NgayBatDau.ToString("dd/MM/yyyy"),
                        NgayKetThuc = h.NgayKetThuc.ToString("dd/MM/yyyy"),
                        GiaPhong = h.MaPhongNavigation != null ? h.MaPhongNavigation.GiaPhong : 0,
                        TienCoc = h.TienCoc ?? 0,
                        TrangThai = h.TrangThai ?? "Chưa xác định"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = contracts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lọc hợp đồng: " + ex.Message });
            }
        }

        // API lọc hợp đồng sắp hết hạn
        [HttpGet]
        public async Task<IActionResult> FilterByExpiration(string range)
        {
            try
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                DateOnly endDate;

                // Xác định khoảng thời gian lọc
                switch (range?.ToLower())
                {
                    case "1week":
                        endDate = today.AddDays(7);
                        break;
                    case "1month":
                        endDate = today.AddMonths(1);
                        break;
                    case "2months":
                        endDate = today.AddMonths(2);
                        break;
                    default:
                        return Json(new { success = false, message = "Không hợp lệ!" });
                }

                var contracts = await _context.HopDongs
                    .Include(h => h.MaKhachNavigation)
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => h.TrangThai == "Còn hiệu lực" &&
                               h.NgayKetThuc >= today &&
                               h.NgayKetThuc <= endDate)
                    .Select(h => new
                    {
                        MaHopDong = h.MaHopDong,
                        TenKhach = h.MaKhachNavigation != null ? h.MaKhachNavigation.HoTen : "Chưa xác định",
                        TenPhong = h.MaPhongNavigation != null ? h.MaPhongNavigation.TenPhong : "Chưa xác định",
                        NgayBatDau = h.NgayBatDau.ToString("dd/MM/yyyy"),
                        NgayKetThuc = h.NgayKetThuc.ToString("dd/MM/yyyy"),
                        GiaPhong = h.MaPhongNavigation != null ? h.MaPhongNavigation.GiaPhong : 0,
                        TienCoc = h.TienCoc ?? 0,
                        TrangThai = h.TrangThai ?? "Chưa xác định"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = contracts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API lấy chi tiết hợp đồng
        [HttpGet]
        public async Task<IActionResult> GetContractDetail(int id)
        {
            try
            {
                var contract = await _context.HopDongs
                    .Include(h => h.MaKhachNavigation)
                    .Include(h => h.MaPhongNavigation)
                        .ThenInclude(p => p.ChiTietPhong)
                    .FirstOrDefaultAsync(h => h.MaHopDong == id);

                if (contract == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hợp đồng!" });
                }

                var contractDetail = new
                {
                    maHopDong = contract.MaHopDong,
                    ngayBatDau = contract.NgayBatDau.ToString("dd/MM/yyyy"),
                    ngayKetThuc = contract.NgayKetThuc.ToString("dd/MM/yyyy"),
                    tienCoc = contract.TienCoc ?? 0,
                    trangThai = contract.TrangThai ?? "Chưa xác định",

                    tenant = new
                    {
                        hoTen = contract.MaKhachNavigation?.HoTen ?? "Chưa xác định",
                        cccd = contract.MaKhachNavigation?.Cccd ?? "Chưa xác định",
                        soDienThoai = contract.MaKhachNavigation?.SoDienThoai ?? "Chưa xác định",
                        ngaySinh = contract.MaKhachNavigation?.NgaySinh?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật",
                        diaChi = contract.MaKhachNavigation?.DiaChi ?? "Chưa cập nhật"
                    },
                    room = new
                    {
                        tenPhong = contract.MaPhongNavigation?.TenPhong ?? "Chưa xác định",
                        loaiPhong = contract.MaPhongNavigation?.ChiTietPhong?.LoaiPhong ?? "Chưa phân loại",
                        dienTich = contract.MaPhongNavigation?.ChiTietPhong?.DienTich ?? 0,
                        giaPhong = contract.MaPhongNavigation?.GiaPhong ?? 0
                    }
                };

                return Json(new { success = true, data = contractDetail });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải chi tiết hợp đồng: " + ex.Message });
            }
        }

        // API kết thúc hợp đồng
        [HttpPost]
        public async Task<IActionResult> TerminateContract([FromBody] int id)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var contract = await _context.HopDongs
                    .Include(h => h.MaPhongNavigation)
                    .FirstOrDefaultAsync(h => h.MaHopDong == id);

                if (contract == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hợp đồng!" });
                }

                // Kiểm tra hợp đồng đã kết thúc chưa
                if (contract.TrangThai != null && contract.TrangThai.Trim() == "Đã kết thúc")
                {
                    return Json(new { success = false, message = "Hợp đồng này đã được kết thúc trước đó!" });
                }

                // Cập nhật trạng thái hợp đồng và phòng
                contract.TrangThai = "Đã kết thúc";
                if (contract.MaPhongNavigation != null)
                {
                    contract.MaPhongNavigation.TrangThai = "Trống";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Kết thúc hợp đồng thành công! Phòng đã được giải phóng."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi kết thúc hợp đồng: " + ex.Message
                });
            }
        }

        // Trang tạo hợp đồng
        public async Task<IActionResult> Create(int? roomId)
        {
            ViewBag.RoomId = roomId;

            if (roomId.HasValue)
            {
                var room = await _context.Phongs
                    .Include(p => p.ChiTietPhong)
                    .FirstOrDefaultAsync(p => p.MaPhong == roomId.Value);

                if (room != null)
                {
                    ViewBag.RoomInfo = new
                    {
                        MaPhong = room.MaPhong,
                        TenPhong = room.TenPhong ?? "Không xác định",
                        GiaPhong = room.GiaPhong,
                        DienTich = room.ChiTietPhong?.DienTich ?? 0,
                        LoaiPhong = room.ChiTietPhong?.LoaiPhong ?? "Chưa phân loại"
                    };
                }
            }

            return View();
        }

        // API tạo hợp đồng với khách thuê
        [HttpPost]
        public async Task<IActionResult> CreateWithTenant([FromBody] CreateContractRequest request)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Kiểm tra phòng tồn tại và trống
                var room = await _context.Phongs.FindAsync(request.MaPhong);
                if (room == null)
                {
                    return Json(new { success = false, message = "Phòng không tồn tại!" });
                }

                if (room.TrangThai?.ToLower() != "trống")
                {
                    return Json(new { success = false, message = "Phòng đã được thuê hoặc không khả dụng!" });
                }

                int maKhach;

                // Kiểm tra khách thuê đã tồn tại (theo CCCD hoặc SĐT)
                var existingTenant = await _context.KhachThues
                    .FirstOrDefaultAsync(k => k.Cccd == request.Tenant.Cccd || k.SoDienThoai == request.Tenant.SoDienThoai);

                if (existingTenant != null)
                {
                    // Sử dụng khách thuê có sẵn
                    maKhach = existingTenant.MaKhach;
                }
                else
                {
                    // Tạo khách thuê mới
                    var newTenant = new QuanLyPhongTro.Models.KhachThue
                    {
                        HoTen = request.Tenant.HoTen?.Trim() ?? "",
                        Cccd = request.Tenant.Cccd?.Trim() ?? "",
                        SoDienThoai = request.Tenant.SoDienThoai?.Trim() ?? "",
                        DiaChi = request.Tenant.DiaChi?.Trim(),
                        NgaySinh = !string.IsNullOrEmpty(request.Tenant.NgaySinh) && DateTime.TryParse(request.Tenant.NgaySinh, out DateTime ngaySinh)
                            ? DateOnly.FromDateTime(ngaySinh)
                            : null
                    };

                    _context.KhachThues.Add(newTenant);
                    await _context.SaveChangesAsync(); // Lưu để lấy MaKhach
                    maKhach = newTenant.MaKhach;
                }

                // Tạo hợp đồng mới - ĐÂY LÀ THAY ĐỔI CHÍNH
                var hopDong = new HopDong
                {
                    MaKhach = maKhach,
                    MaPhong = request.MaPhong,
                    NgayBatDau = DateOnly.FromDateTime(DateTime.Parse(request.NgayBatDau)),
                    NgayKetThuc = DateOnly.FromDateTime(DateTime.Parse(request.NgayKetThuc)),
                    TienCoc = request.TienCoc,
                    TrangThai = "Còn hiệu lực"
                };

                _context.HopDongs.Add(hopDong);
                room.TrangThai = "Đang thuê"; // Cập nhật trạng thái phòng

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Tạo hợp đồng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tạo hợp đồng: " + ex.Message });
            }
        }
    }

    // DTO classes
    public class CreateContractRequest
    {
        public int MaPhong { get; set; }
        public string NgayBatDau { get; set; } = string.Empty;
        public string NgayKetThuc { get; set; } = string.Empty;
        public decimal TienCoc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public TenantRequest Tenant { get; set; } = new TenantRequest();
    }

    public class TenantRequest
    {
        public int? MaKhach { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Cccd { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? NgaySinh { get; set; }
        public bool IsExisting { get; set; } = false;
    }
}