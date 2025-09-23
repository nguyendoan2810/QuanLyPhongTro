using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class KhachThue
{
    public int MaKhach { get; set; }

    public string HoTen { get; set; } = null!;

    public string Cccd { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? DiaChi { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
