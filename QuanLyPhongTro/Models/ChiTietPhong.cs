using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class ChiTietPhong
{
    public int MaChiTietPhong { get; set; }

    public int? MaPhong { get; set; }

    public string? DiaChi { get; set; }

    public int? Tang { get; set; }

    public decimal? DienTich { get; set; }

    public string? LoaiPhong { get; set; }

    public string? MoTa { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}
