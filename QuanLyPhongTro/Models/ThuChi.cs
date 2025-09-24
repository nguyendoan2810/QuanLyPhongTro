using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class ThuChi
{
    public int MaTc { get; set; }

    public DateTime? Ngay { get; set; }

    public string? Loai { get; set; }

    public decimal SoTien { get; set; }

    public string? NoiDung { get; set; }

    public int? MaHd { get; set; }

    public virtual HoaDon? MaHdNavigation { get; set; }
}
