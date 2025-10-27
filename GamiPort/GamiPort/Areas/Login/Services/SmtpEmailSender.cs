using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace GamiPort.Areas.Login.Services
{
	public class SmtpEmailSender : IEmailSender
	{
		private readonly IConfiguration _config;

		public SmtpEmailSender(IConfiguration config)
		{
			_config = config;
		}

		public async Task SendAsync(string to, string subject, string body)
		{
			var host = _config["Smtp:Host"];
			var portStr = _config["Smtp:Port"];
			var user = _config["Smtp:Username"];
			var pass = _config["Smtp:Password"];
			var fromRaw = _config["Smtp:From"];

			if (string.IsNullOrWhiteSpace(host) ||
				string.IsNullOrWhiteSpace(portStr) ||
				string.IsNullOrWhiteSpace(user) ||
				string.IsNullOrWhiteSpace(pass) ||
				string.IsNullOrWhiteSpace(fromRaw))
			{
				throw new InvalidOperationException("SMTP 設定(Smtp:*)不完整，請檢查 appsettings.json。");
			}

			// 允許 "GamiPort <addr@...>" or 純 email
			MailAddress from;
			try
			{
				if (fromRaw.Contains("<") && fromRaw.Contains(">"))
				{
					var email = fromRaw[(fromRaw.IndexOf('<') + 1)..fromRaw.IndexOf('>')].Trim();
					var display = fromRaw[..fromRaw.IndexOf('<')].Trim().TrimEnd();
					from = new MailAddress(email, display);
				}
				else
				{
					from = new MailAddress(fromRaw);
				}
			}
			catch
			{
				// 萬一格式被打壞，退回純 email
				from = new MailAddress(user);
			}

			var port = int.TryParse(portStr, out var p) ? p : 587;

			try
			{
				using var client = new SmtpClient(host, port)
				{
					EnableSsl = true,                       // Gmail 必須
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(user, pass),
					Timeout = 20000
				};

				using var mail = new MailMessage
				{
					From = from,
					Subject = subject,
					Body = body,
					IsBodyHtml = false
				};
				mail.To.Add(new MailAddress(to));

				await client.SendMailAsync(mail);
			}
			catch (SmtpException ex)
			{
				// 把最有用的資訊拋回去讓你看到
				throw new Exception($"SMTP 寄信失敗：{ex.StatusCode} - {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception("SMTP 寄信時發生例外：" + ex.Message, ex);
			}
		}
	}
}
