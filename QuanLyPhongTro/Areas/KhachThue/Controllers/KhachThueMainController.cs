using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class KhachThueMainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
