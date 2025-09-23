using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    public class HoaDonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
