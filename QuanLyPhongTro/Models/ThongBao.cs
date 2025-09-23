using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.Models;

public partial class ThongBao
{
    public int MaTb { get; set; }

    public int MaTk { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime NgayGui { get; set; }

    public string? Loai { get; set; }

    public virtual TaiKhoan MaTkNavigation { get; set; } = null!;
}
