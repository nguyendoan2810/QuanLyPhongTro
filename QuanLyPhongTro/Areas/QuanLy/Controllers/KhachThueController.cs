using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System.Linq;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class KhachThueController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public KhachThueController(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}




