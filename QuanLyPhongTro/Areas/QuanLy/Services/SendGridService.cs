using SendGrid;
using SendGrid.Helpers.Mail;

namespace QuanLyPhongTro.Areas.QuanLy.Services
{
    public class SendGridService
    {
        private readonly IConfiguration _config;

        public SendGridService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var senderEmail = _config["SendGrid:SenderEmail"];
            var senderName = _config["SendGrid:SenderName"];

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail, senderName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "Mật khẩu mới", htmlContent);

            var response = await client.SendEmailAsync(msg);
            Console.WriteLine($"📤 SendGrid status: {response.StatusCode}");

            if ((int)response.StatusCode >= 400)
                throw new Exception("Gửi email thất bại qua SendGrid.");
        }
    }
}
