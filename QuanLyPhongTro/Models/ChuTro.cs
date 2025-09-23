using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class ChuTro
{
    public int MaChuTro { get; set; }

    public string HoTen { get; set; } = null!;

    public string? Cccd { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
