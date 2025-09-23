using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
