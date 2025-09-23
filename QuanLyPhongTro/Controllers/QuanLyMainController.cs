using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Controllers
{
    public class QuanLyMainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
