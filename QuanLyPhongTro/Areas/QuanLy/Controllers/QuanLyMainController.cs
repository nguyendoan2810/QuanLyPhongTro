using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    public class QuanLyMainController : Controller
    {
        [Area("QuanLy")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
