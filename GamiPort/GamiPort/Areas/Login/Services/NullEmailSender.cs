namespace GamiPort.Areas.Login.Services
{
	public class NullEmailSender : IEmailSender
	{
		public Task SendAsync(string to, string subject, string htmlBody)
		{ 
			// 開發期間先不寄信
            return Task.CompletedTask;
		}
	}
}


