namespace QuanLyPhongTro.Areas.QuanLy.Models.ViewModels
{
    public class TaiKhoanViewModel
    {
        public int MaTk { get; set; }
        public string HoTen { get; set; } = "";
        public string? Email { get; set; }
        public string? VaiTro { get; set; }
        public string? TenPhong { get; set; }
        public string? DiaChi { get; set; }
    }
}
