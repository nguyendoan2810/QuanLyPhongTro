using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class DichVu
{
    public int MaDv { get; set; }

    public string TenDv { get; set; } = null!;

    public decimal DonGia { get; set; }

    public virtual ICollection<ChiSoDichVu> ChiSoDichVus { get; set; } = new List<ChiSoDichVu>();

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
}
