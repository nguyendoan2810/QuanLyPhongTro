using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    public class HoaDonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
