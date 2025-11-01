using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class HoaDonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
