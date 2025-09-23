using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class HoaDon
{
    public int MaHd { get; set; }

    public int? MaHopDong { get; set; }

    public int? Thang { get; set; }

    public int? Nam { get; set; }

    public decimal TongTien { get; set; }

    public string? TrangThai { get; set; }

    public DateOnly? NgayTao { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual HopDong? MaHopDongNavigation { get; set; }

    public virtual ICollection<ThuChi> ThuChis { get; set; } = new List<ThuChi>();
}
