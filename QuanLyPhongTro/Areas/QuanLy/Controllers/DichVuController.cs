using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class DichVuController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public DichVuController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DichVu model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDv) || model.DonGia <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            _context.DichVus.Add(model);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                data = new
                {
                    MaDv = model.MaDv,
                    TenDv = model.TenDv,
                    DonGia = model.DonGia.ToString("N0")
                }
            });
        }

        [HttpGet]
        public IActionResult ReloadPartial()
        {
            return ViewComponent("DichVu");
        }
    }
}
