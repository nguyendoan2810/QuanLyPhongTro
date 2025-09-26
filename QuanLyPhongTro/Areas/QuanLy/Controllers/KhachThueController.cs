using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class KhachThueController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
