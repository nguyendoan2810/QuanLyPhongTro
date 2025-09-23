using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class ChiSoDichVu
{
    public int MaChiSo { get; set; }

    public int MaHopDong { get; set; }

    public int MaDv { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    public int? ChiSoCu { get; set; }

    public int? ChiSoMoi { get; set; }

    public virtual DichVu MaDvNavigation { get; set; } = null!;

    public virtual HopDong MaHopDongNavigation { get; set; } = null!;
}
