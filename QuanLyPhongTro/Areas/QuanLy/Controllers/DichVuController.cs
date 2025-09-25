using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class DichVuController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public DichVuController(QuanLyPhongTroContext context)
        {
            _context = context;
        }
       
        public IActionResult Index()
        {
            return View();
        }
    }
}
