using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class ThuChiViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public ThuChiViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? month, int? year)
        {
            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;

            //Lay danh sach thu chi
            var thuChiList = _context.ThuChis
                .Where(tc => tc.Ngay.HasValue 
                          && tc.Ngay.Value.Month == currentMonth
                          && tc.Ngay.Value.Year == currentYear)
                .OrderByDescending(tc => tc.Ngay).ToList();

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

            ViewBag.SelectedMonth = currentMonth;
            ViewBag.SelectedYear = currentYear;

            return View("~/Areas/QuanLy/Views/ThuChi/Index.cshtml");
        }
    }
}
