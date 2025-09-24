using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    public class KhachThueMainController : Controller
    {
        [Area("KhachThue")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
