namespace QuanLyPhongTro.Areas.QuanLy.Models.ViewModels
{
    public class HoaDonViewModel
    {
        public string MaHoaDonHienThi { get; set; }
        public string TenPhong { get; set; }
        public string TenKhachThue { get; set; }
        public string KyHoaDon { get; set; }
        public decimal TongTien { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? HanThanhToan { get; set; }
        public string TrangThai { get; set; }
    }
}
