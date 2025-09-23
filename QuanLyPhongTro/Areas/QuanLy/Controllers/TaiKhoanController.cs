using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    public class TaiKhoanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
