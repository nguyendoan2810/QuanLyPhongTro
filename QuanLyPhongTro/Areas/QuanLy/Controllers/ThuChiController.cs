using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System.Linq;

namespace QuanLyPhongTro.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    public class ThuChiController : Controller
    {
        private readonly QuanLyPhongTroContext _context;

        public ThuChiController(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            //Lay danh sach thu chi
            var thuChiList = _context.ThuChis.OrderByDescending(tc => tc.Ngay).ToList();

            //tong thu / tong chi / loi nhuan
            var tongThu = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "thu").Sum(tc => tc.SoTien);

            var tongChi = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "chi").Sum(tc => tc.SoTien);

            var loiNhuan = tongThu - tongChi;

            //truyen sang view
            ViewBag.TongThu = tongThu;
            ViewBag.TongChi = tongChi;
            ViewBag.LoiNhuan = loiNhuan;

            ViewBag.ThuList = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "thu").OrderByDescending(x => x.Ngay).ToList();
            ViewBag.ChiList = thuChiList.Where(tc => tc.Loai != null && tc.Loai.Trim().ToLower() == "chi").OrderByDescending(x => x.Ngay).ToList();

            return View();
        }     
    }
}
