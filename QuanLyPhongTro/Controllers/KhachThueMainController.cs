using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Controllers
{
    public class KhachThueMainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
