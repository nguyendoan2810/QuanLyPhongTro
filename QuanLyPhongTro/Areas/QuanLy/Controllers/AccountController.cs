using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
