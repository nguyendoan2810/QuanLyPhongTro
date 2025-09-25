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
        public IActionResult Create(string loai)
        {
            var model = new ThuChi
            {
                Loai = loai,             // "Thu" hoặc "Chi"
                Ngay = DateTime.Now      // mặc định ngày hiện tại
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ThuChi model)
        {
            if (string.IsNullOrWhiteSpace(model.NoiDung))
                ModelState.AddModelError("NoiDung", "Vui lòng nhập nội dung");
            if (model.SoTien <= 0)
                ModelState.AddModelError("SoTien", "Số tiền phải lớn hơn 0");

            if (!ModelState.IsValid)
            {
                // trả về trang hiện tại nếu lỗi
                return RedirectToAction("Index", "QuanLyMain", new { area = "QuanLy" });
            }

            model.Ngay ??= DateTime.Now;
            _context.ThuChis.Add(model);
            _context.SaveChanges();

            // sau khi lưu xong quay lại trang tổng
            return RedirectToAction("Index", "QuanLyMain", new { area = "QuanLy" });          
        }
    }
}
