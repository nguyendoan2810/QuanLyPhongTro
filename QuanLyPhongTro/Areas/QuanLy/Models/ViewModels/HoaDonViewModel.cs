using System.Collections.Generic;
namespace QuanLyPhongTro.Areas.QuanLy.Models.ViewModels
{
    public class HoaDonViewModel
    {
        public int MaHd { get; set; }
        public string MaHoaDonHienThi { get; set; }
        public string TenPhong { get; set; }
        public string TenKhachThue { get; set; }
        public string KyHoaDon { get; set; }
        public decimal TongTien { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public string TrangThai { get; set; }
    }

    public class TaoHoaDonRequest
    {
        public int MaHd { get; set; } // dùng cho cập nhật
        public int MaHopDong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal ThanhTienThangNay { get; set; }
        public List<ChiTietHoaDonRequest> ChiTiet { get; set; }

        // Chỉ số (nếu người dùng nhập)
        public ChiSoModel ChiSoDien { get; set; }
        public ChiSoModel ChiSoNuoc { get; set; }
    }

    public class ChiTietHoaDonRequest
    {
        public int MaDv { get; set; }         // mã dịch vụ (0 cho tiền phòng nếu bạn muốn)
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class ChiSoModel
    {
        public int Cu { get; set; }
        public int Moi { get; set; }
    }
}