namespace GameSpace.Models.ViewModels
{
	public class LoginSuccessVM
	{
		public int ManagerId { get; set; }
		public string ManagerName { get; set; } = "";
		public List<string> Positions { get; set; } = new();
	}
}
