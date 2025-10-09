using Microsoft.AspNetCore.Mvc;
using QuanLyPhongTro.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyPhongTro.Areas.QuanLy.ViewComponents
{
    public class DichVuViewComponent : ViewComponent
    {
        private readonly QuanLyPhongTroContext _context;

        public DichVuViewComponent(QuanLyPhongTroContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //Lay danh sach dich vu
            var dichVuList = _context.DichVus.ToList();

            //truyen sang view
            ViewBag.DichVuList = dichVuList.Where(tc => tc.TenDv != null).ToList();


            return View("~/Areas/QuanLy/Views/DichVu/Index.cshtml");
        }
    }
}


