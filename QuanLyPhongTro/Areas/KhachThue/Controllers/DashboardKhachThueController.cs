using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    public class DashboardKhachThueController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
