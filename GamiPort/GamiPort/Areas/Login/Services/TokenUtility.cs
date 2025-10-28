using System.Security.Cryptography;

namespace GamiPort.Areas.Login.Services
{
	public static class TokenUtility
	{
		// 產生 URL-safe 的隨機 Token（約 43 字元）
		public static string NewUrlSafeToken(int numBytes = 32)
		{
			var bytes = RandomNumberGenerator.GetBytes(numBytes);
			var base64 = Convert.ToBase64String(bytes);
			return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
		}
	}
}
