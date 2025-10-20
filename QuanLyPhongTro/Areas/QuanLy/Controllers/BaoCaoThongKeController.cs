using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System;
using System.Linq;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{

    [Area("QuanLy")]
    public class BaoCaoThongKeController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public BaoCaoThongKeController(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
