using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class Phong
{
    public int MaPhong { get; set; }

    public string TenPhong { get; set; } = null!;

    public decimal GiaPhong { get; set; }

    public string TrangThai { get; set; } = null!;

    public int? MaChuTro { get; set; }

    public virtual ChiTietPhong? ChiTietPhong { get; set; }

    public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();

    public virtual ChuTro? MaChuTroNavigation { get; set; }
}
