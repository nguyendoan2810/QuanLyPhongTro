using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    public class PhongController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
