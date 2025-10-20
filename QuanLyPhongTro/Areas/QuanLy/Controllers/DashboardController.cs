using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System;
using System.Linq;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class DashboardController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public DashboardController(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
           
            return View();
        }
       
    }
}