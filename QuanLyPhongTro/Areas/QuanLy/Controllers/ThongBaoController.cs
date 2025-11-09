using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class ThongBaoController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public ThongBaoController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        [HttpGet]
       
        [ActionName("ReloadPartial")]
        public IActionResult ReloadPartialAction(string loc = "all")
        {
            return ViewComponent("ThongBao", new { loc });
        }

        [HttpPost]
        public IActionResult Tao(string maKhach, string maPhong, string loai, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return Json(new { success = false, message = "⚠️ Vui lòng nhập nội dung thông báo!" });

            var maChuTro = HttpContext.Session.GetInt32("MaChuTro");
            if (maChuTro == null)
                return Json(new { success = false, message = "Không xác định được chủ trọ đang đăng nhập!" });

            try
            {
                List<int> danhSachMaKhach = new();

                // 🔹 Trường hợp 3: Gửi cho tất cả khách thuê
                if (maPhong == "ALL")
                {
                    danhSachMaKhach = (from hd in _context.HopDongs
                                       join p in _context.Phongs on hd.MaPhong equals p.MaPhong
                                       where p.MaChuTro == maChuTro && hd.MaKhach != null
                                       select hd.MaKhach.Value).Distinct().ToList();



                    if (!danhSachMaKhach.Any())
                        return Json(new { success = false, message = "Không có khách thuê nào để gửi!" });

                    return GuiThongBaoNhieuKhach(danhSachMaKhach, noiDung, loai);
                }

                // 🔹 Trường hợp 2: Gửi cho tất cả khách trong 1 phòng cụ thể
                if (maKhach == "ALL" && int.TryParse(maPhong, out int idPhong))
                {
                    danhSachMaKhach = (from hd in _context.HopDongs
                                       where hd.MaPhong == idPhong && hd.MaKhach != null
                                       select hd.MaKhach.Value).Distinct().ToList();

                    if (!danhSachMaKhach.Any())
                        return Json(new { success = false, message = "Phòng này không có khách thuê!" });

                    return GuiThongBaoNhieuKhach(danhSachMaKhach, noiDung, loai);
                }

                // 🔹 Trường hợp 1: Gửi cho 1 khách cụ thể
                if (!int.TryParse(maKhach, out int ma))
                    return Json(new { success = false, message = "Mã khách không hợp lệ!" });

                var hopDong = (from hd in _context.HopDongs
                               join p in _context.Phongs on hd.MaPhong equals p.MaPhong
                               where hd.MaKhach == ma && p.MaChuTro == maChuTro
                               select hd).FirstOrDefault();

                if (hopDong == null)
                    return Json(new { success = false, message = "Khách này không thuộc phòng bạn quản lý!" });

                var tk = _context.TaiKhoans.FirstOrDefault(t => t.MaKhach == ma);
                if (tk == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản khách thuê!" });

                _context.ThongBaos.Add(new ThongBao
                {
                    MaTk = tk.MaTk,
                    NoiDung = noiDung,
                    Loai = loai,
                    NgayGui = DateTime.Now
                });
                _context.SaveChanges();

                var nguoiNhan = _context.KhachThues.FirstOrDefault(k => k.MaKhach == ma)?.HoTen;
                return Json(new { success = true, message = $"✅ Gửi thông báo cho {nguoiNhan} thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }

        private IActionResult GuiThongBaoNhieuKhach(List<int> danhSachMaKhach, string noiDung, string loai)
        {
            foreach (var maKh in danhSachMaKhach)
            {
                var tk = _context.TaiKhoans.FirstOrDefault(t => t.MaKhach == maKh);
                if (tk != null)
                {
                    _context.ThongBaos.Add(new ThongBao
                    {
                        MaTk = tk.MaTk,
                        NoiDung = noiDung,
                        Loai = loai,
                        NgayGui = DateTime.Now
                    });
                }
            }

            _context.SaveChanges();
            return Json(new { success = true, message = $"✅ Đã gửi cho {danhSachMaKhach.Count} khách thuê!" });
        }
    }
}
