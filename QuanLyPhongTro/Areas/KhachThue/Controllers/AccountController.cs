using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
