using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class DichVuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
