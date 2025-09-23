using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    public class DichVuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
