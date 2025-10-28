using Microsoft.EntityFrameworkCore;

namespace GameSpace.Models
{

	public partial class GameSpacedatabaseContext : DbContext
	{
		public GameSpacedatabaseContext() { 
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{ 
			IConfigurationRoot Configuration =
					new ConfigurationBuilder()
					.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
					.AddJsonFile("appsettings.json")
					.Build();
				optionsBuilder.UseSqlServer(Configuration.GetConnectionString("GameSpace"));
			}
		}
	}
}
