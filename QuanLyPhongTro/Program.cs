using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Areas.QuanLy.Services;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Kết nối SQL Server
            builder.Services.AddDbContext<QuanLyPhongTroContext>(options => options
            .UseSqlServer(builder.Configuration.GetConnectionString("QuanLyPhongTroConnectionString")));

            // Đăng ký SendGridService để sử dụng trong các controller
            builder.Services.AddTransient<SendGridService>();

            // Session (cần cho lưu Session thông tin user)
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
