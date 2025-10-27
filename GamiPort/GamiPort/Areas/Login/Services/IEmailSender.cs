namespace GamiPort.Areas.Login.Services
{
	public interface IEmailSender
	{
		Task SendAsync(string to, string subject, string htmlBody);
	}
}
