using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class BaoCaoThongKeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
