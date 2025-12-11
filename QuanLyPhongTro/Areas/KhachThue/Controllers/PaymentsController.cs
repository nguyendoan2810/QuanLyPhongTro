using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Models.Momo;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyPhongTro.Areas.KhachThue.Controllers
{
    [Area("KhachThue")]
    [Route("KhachThue/[controller]/[action]")]
    public class PaymentsController : Controller
    {
        private readonly QuanLyPhongTroContext _context;
        private readonly IConfiguration _config;

        public PaymentsController(QuanLyPhongTroContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Tạo QR thanh toán MoMo
        [HttpPost]
        public async Task<IActionResult> CreateQr([FromBody] CreateQrRequest req)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.ChiTietPhong)
                .FirstOrDefaultAsync(h => h.MaHd == req.MaHd);

            if (hoaDon == null)
                return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

            if (hoaDon.TrangThai == "Đã thanh toán")
                return Json(new { success = false, message = "Hóa đơn đã thanh toán." });

            // Lấy amount từ DB (decimal) và chuyển thành chuỗi nguyên không có dấu thập phân
            decimal amount = hoaDon.TongTien;
            string amountStr = ((long)amount).ToString(); // MoMo yêu cầu "10000" (không "10000.00")

            var phong = hoaDon.MaHopDongNavigation?.MaPhongNavigation;
            string orderInfo = $"Thanh toán hóa đơn phòng {phong?.TenPhong} tháng {hoaDon.Thang}/{hoaDon.Nam}";

            // Cấu hình MoMo
            string partnerCode = _config["Momo:PartnerCode"];
            string accessKey = _config["Momo:AccessKey"];
            string secretKey = _config["Momo:SecretKey"];
            string endpoint = _config["Momo:CreateEndpoint"];
            string returnUrl = _config["Momo:ReturnUrl"];
            string notifyUrl = _config["Momo:NotifyUrl"];

            string orderId = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = hoaDon.MaHd.ToString();

            // Tạo signature và request body theo chuẩn MoMo
            string requestType = "captureWallet";
            string partnerName = "MoMo Payment";
            string storeId = "MomoTestStore";

            // rawHash: chú ý thứ tự và tên trường phải chính xác
            string rawHash = $"accessKey={accessKey}&amount={amountStr}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            string signature = SignHmacSHA256(rawHash, secretKey);

            var requestBody = new
            {
                partnerCode,
                partnerName,
                storeId,
                requestId,
                amount = amountStr,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                extraData,
                requestType,
                signature
            };

            // In requestBody để debug (bỏ hoặc comment khi deploy)
            Console.WriteLine("=== MoMo RequestBody ===");
            Console.WriteLine(JsonConvert.SerializeObject(requestBody, Formatting.Indented));

            using var client = new HttpClient();
            var response = await client.PostAsync(endpoint,
                new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("=== MoMo Response ===");
            Console.WriteLine(content);

            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponse>(content);
            if (momoResponse == null)
                return Json(new { success = false, message = "Không thể đọc phản hồi MoMo." });

            if (momoResponse.resultCode != 0)
                return Json(new { success = false, message = $"Lỗi MoMo: {momoResponse.message} (Code {momoResponse.resultCode})" });

            return Json(new { success = true, payUrl = momoResponse.payUrl });
        }

        private static string SignHmacSHA256(string text, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }

        [HttpPost]
        public async Task<IActionResult> Notify(MomoNotifyRequest data)
        {
            try
            {
                if (!int.TryParse(data.extraData, out int maHd))
                    return Ok(new { message = "Invalid MaHd" });

                var hoaDon = await _context.HoaDons
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaKhachNavigation)
                            .ThenInclude(k => k.TaiKhoans)
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                            .ThenInclude(p => p.ChiTietPhong)
                    .FirstOrDefaultAsync(h => h.MaHd == maHd);

                if (hoaDon == null)
                    return Ok(new { message = "Invoice not found" });

                // Chỉ cập nhật khi resultCode = 0 (thành công)
                if (data.resultCode == 0 && hoaDon.TrangThai != "Đã thanh toán")
                {
                    hoaDon.TrangThai = "Đã thanh toán";

                    var phong = hoaDon.MaHopDongNavigation?.MaPhongNavigation;
                    var diaChi = phong?.ChiTietPhong?.DiaChi ?? "Không rõ địa chỉ";
                    var tenPhong = phong?.TenPhong ?? "Phòng trọ";

                    _context.ThuChis.Add(new ThuChi
                    {
                        Ngay = DateTime.Now,
                        Loai = "Thu",
                        SoTien = hoaDon.TongTien,
                        NoiDung = $"{tenPhong} - {diaChi} - thanh toán hóa đơn tháng {hoaDon.Thang}/{hoaDon.Nam}",
                        MaHd = hoaDon.MaHd
                    });

                    var khach = hoaDon.MaHopDongNavigation?.MaKhachNavigation;
                    var maTk = khach?.TaiKhoans.FirstOrDefault()?.MaTk;
                    if (maTk.HasValue)
                    {
                        _context.ThongBaos.Add(new ThongBao
                        {
                            MaTk = maTk.Value,
                            NoiDung = $"Bạn đã thanh toán hóa đơn tháng {hoaDon.Thang}/{hoaDon.Nam}",
                            NgayGui = DateTime.Now,
                            Loai = "HoaDon"
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Received", resultCode = 0 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notify error: {ex.Message}");
                return Ok(new { message = "Error", resultCode = 1 });
            }
        }
    }
}