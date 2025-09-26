using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class PhongController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
