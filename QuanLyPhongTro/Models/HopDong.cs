using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class HopDong
{
    public int MaHopDong { get; set; }

    public int? MaKhach { get; set; }

    public int? MaPhong { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public decimal? TienCoc { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChiSoDichVu> ChiSoDichVus { get; set; } = new List<ChiSoDichVu>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual KhachThue? MaKhachNavigation { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}
