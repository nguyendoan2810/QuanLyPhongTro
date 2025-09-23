using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class TaiKhoan
{
    public int MaTk { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public int? MaKhach { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public bool? TrangThai { get; set; }

    public int? MaChuTro { get; set; }

    public string? Cccd { get; set; }

    public virtual ChuTro? MaChuTroNavigation { get; set; }

    public virtual KhachThue? MaKhachNavigation { get; set; }

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
