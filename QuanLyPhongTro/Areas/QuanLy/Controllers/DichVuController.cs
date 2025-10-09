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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int MaDV, string TenDv, decimal DonGia)
        {
            var item = _context.DichVus.Find(MaDV);
            if (item == null) return NotFound("Không tìm thấy dịch vụ này.");

            item.TenDv = TenDv;
            item.DonGia = DonGia;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int maDv)
        {
            var item = _context.DichVus.Find(maDv);
            if (item == null) return NotFound("Không tìm thấy dịch vụ này");

            _context.DichVus.Remove(item);
            _context.SaveChanges();

            return Ok();
        }
    }

}
