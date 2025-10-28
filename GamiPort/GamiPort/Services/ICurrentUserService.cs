namespace GamiPort.Services
{
	public interface ICurrentUserService
	{
		bool IsAuthenticated { get; }
		int? UserId { get; }
		string? NickName { get; }
		string? Email { get; }
	}
}
