using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class TaiKhoanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
