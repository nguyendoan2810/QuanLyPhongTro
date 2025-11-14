using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongTro.Areas.KhachThue.ViewComponents
{
    public class TaiKhoanKhachThueViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Areas/KhachThue/Views/TaiKhoanKhachThue/Index.cshtml");
        }
    }
}
