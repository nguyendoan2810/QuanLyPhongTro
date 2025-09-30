using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System.Linq;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class ThuChiController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public ThuChiController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {           
            return View();
        }

        [HttpGet]
        public IActionResult FilterByMonth(int month, int year)
        {
            if (month < 1 || month > 12) return BadRequest("Tháng không hợp lệ!");
            if (year == 0) year = DateTime.Now.Year;

            return ViewComponent("ThuChi", new { month = month, year = year });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ThuChi model)
        {
            model.Loai = model.Loai?.Trim().ToLowerInvariant();
            model.Ngay ??= DateTime.Now;

            if (string.IsNullOrWhiteSpace(model.NoiDung) || model.SoTien <= 0 || (model.Loai != "thu" && model.Loai != "chi"))
                return BadRequest("Dữ liệu không hợp lệ!");

            _context.ThuChis.Add(model);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                data = new
                {
                    MaTc = model.MaTc,
                    NoiDung = model.NoiDung,
                    SoTien = model.SoTien.ToString("N0"),
                    Ngay = model.Ngay?.ToString("dd/MM/yyyy")
                }
            });
        }


        // Trả về ViewComponent để cập nhật danh sách thu chi (dùng cho AJAX)
        [HttpGet]
        public IActionResult ReloadPartial()
        {
            return ViewComponent("ThuChi");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int Id, string NoiDung, decimal SoTien, DateTime Ngay, string Loai)
        {
            var item = _context.ThuChis.Find(Id);
            if (item == null) return NotFound("Không tìm thấy khoản thu/chi.");

            item.NoiDung = NoiDung;
            item.SoTien = SoTien;
            item.Ngay = Ngay;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = _context.ThuChis.Find(id);
            if (item == null) return NotFound("Không tìm thấy khoản thu/chi.");

            _context.ThuChis.Remove(item);
            _context.SaveChanges();

            return Ok();
        }
    }
}