using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class ChiTietHoaDon
{
    public int MaCthd { get; set; }

    public int? MaHd { get; set; }

    public int? MaDv { get; set; }

    public decimal? SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual DichVu? MaDvNavigation { get; set; }

    public virtual HoaDon? MaHdNavigation { get; set; }
}
