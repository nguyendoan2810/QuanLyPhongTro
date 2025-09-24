using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyPhongTro.Models;

public partial class QuanLyPhongTroContext : DbContext
{
    public QuanLyPhongTroContext()
    {
    }

    public QuanLyPhongTroContext(DbContextOptions<QuanLyPhongTroContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiSoDichVu> ChiSoDichVus { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<ChiTietPhong> ChiTietPhongs { get; set; }

    public virtual DbSet<ChuTro> ChuTros { get; set; }

    public virtual DbSet<DichVu> DichVus { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<HopDong> HopDongs { get; set; }

    public virtual DbSet<KhachThue> KhachThues { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<ThuChi> ThuChis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-EJ1JGD5;Initial Catalog=QuanLyPhongTro;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiSoDichVu>(entity =>
        {
            entity.HasKey(e => e.MaChiSo).HasName("PK__ChiSoDic__EBA18E15AB1FC656");

            entity.ToTable("ChiSoDichVu");

            entity.Property(e => e.ChiSoCu).HasDefaultValue(0);
            entity.Property(e => e.ChiSoMoi).HasDefaultValue(0);
            entity.Property(e => e.MaDv).HasColumnName("MaDV");

            entity.HasOne(d => d.MaDvNavigation).WithMany(p => p.ChiSoDichVus)
                .HasForeignKey(d => d.MaDv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiSoDichV__MaDV__5EBF139D");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.ChiSoDichVus)
                .HasForeignKey(d => d.MaHopDong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiSoDich__MaHop__5DCAEF64");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaCthd).HasName("PK__ChiTietH__1E4FA77117384BDF");

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.MaCthd).HasColumnName("MaCTHD");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaDv).HasColumnName("MaDV");
            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.SoLuong)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("([SoLuong]*[DonGia])", false)
                .HasColumnType("decimal(37, 4)");

            entity.HasOne(d => d.MaDvNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaDv)
                .HasConstraintName("FK__ChiTietHoa__MaDV__6C190EBB");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHd)
                .HasConstraintName("FK__ChiTietHoa__MaHD__6B24EA82");
        });

        modelBuilder.Entity<ChiTietPhong>(entity =>
        {
            entity.HasKey(e => e.MaChiTietPhong).HasName("PK__ChiTietP__8BEC796D05B304F1");

            entity.ToTable("ChiTietPhong");

            entity.HasIndex(e => e.MaPhong, "UQ__ChiTietP__20BD5E5A7B2BCD14").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.DienTich).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LoaiPhong).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(500);

            entity.HasOne(d => d.MaPhongNavigation).WithOne(p => p.ChiTietPhong)
                .HasForeignKey<ChiTietPhong>(d => d.MaPhong)
                .HasConstraintName("FK__ChiTietPh__MaPho__4E88ABD4");
        });

        modelBuilder.Entity<ChuTro>(entity =>
        {
            entity.HasKey(e => e.MaChuTro).HasName("PK__ChuTro__D1B7162AC55367FE");

            entity.ToTable("ChuTro");

            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("CCCD");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.HasKey(e => e.MaDv).HasName("PK__DichVu__272586571981502B");

            entity.ToTable("DichVu");

            entity.Property(e => e.MaDv).HasColumnName("MaDV");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenDv)
                .HasMaxLength(50)
                .HasColumnName("TenDV");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HoaDon__2725A6E0276B1AFB");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Chưa thanh toán");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__HoaDon__MaHopDon__6477ECF3");
        });

        modelBuilder.Entity<HopDong>(entity =>
        {
            entity.HasKey(e => e.MaHopDong).HasName("PK__HopDong__36DD43421299EBD4");

            entity.ToTable("HopDong");

            entity.Property(e => e.TienCoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Còn hiệu lực");

            entity.HasOne(d => d.MaKhachNavigation).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.MaKhach)
                .HasConstraintName("FK__HopDong__MaKhach__5535A963");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HopDong__MaPhong__5629CD9C");
        });

        modelBuilder.Entity<KhachThue>(entity =>
        {
            entity.HasKey(e => e.MaKhach).HasName("PK__KhachThu__D0CB8DDD282A8066");

            entity.ToTable("KhachThue");

            entity.HasIndex(e => e.SoDienThoai, "UQ__KhachThu__0389B7BDFCF7B503").IsUnique();

            entity.HasIndex(e => e.Cccd, "UQ__KhachThu__A955A0AA7F688CB4").IsUnique();

            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Phong__20BD5E5BCD71E34F");

            entity.ToTable("Phong");

            entity.Property(e => e.GiaPhong).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenPhong).HasMaxLength(50);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Trống");

            entity.HasOne(d => d.MaChuTroNavigation).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.MaChuTro)
                .HasConstraintName("FK__Phong__MaChuTro__2A164134");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk).HasName("PK__TaiKhoan__272500703FE6EDA5");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0AEFC5895").IsUnique();

            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("CCCD");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro).HasMaxLength(20);

            entity.HasOne(d => d.MaChuTroNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaChuTro)
                .HasConstraintName("FK__TaiKhoan__MaChuT__29221CFB");

            entity.HasOne(d => d.MaKhachNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaKhach)
                .HasConstraintName("FK__TaiKhoan__MaKhac__76969D2E");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaTb).HasName("PK__ThongBao__2725006FFD0DAB13");

            entity.ToTable("ThongBao");

            entity.Property(e => e.MaTb).HasColumnName("MaTB");
            entity.Property(e => e.Loai).HasMaxLength(20);
            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(500);

            entity.HasOne(d => d.MaTkNavigation).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.MaTk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThongBao_TaiKhoan");
        });

        modelBuilder.Entity<ThuChi>(entity =>
        {
            entity.HasKey(e => e.MaTc).HasName("PK__ThuChi__27250068FA252807");

            entity.ToTable("ThuChi");

            entity.Property(e => e.MaTc).HasColumnName("MaTC");
            entity.Property(e => e.Loai).HasMaxLength(20);
            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.Ngay).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NoiDung).HasMaxLength(200);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ThuChis)
                .HasForeignKey(d => d.MaHd)
                .HasConstraintName("FK__ThuChi__MaHD__71D1E811");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
