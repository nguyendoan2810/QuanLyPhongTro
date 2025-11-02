using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class HoaDonKhachThueController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
