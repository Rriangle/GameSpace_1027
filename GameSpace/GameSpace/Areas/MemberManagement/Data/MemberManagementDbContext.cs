using GameSpace.Areas.MemberManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MemberManagement.Data
{
	public class MemberManagementDbContext : DbContext
	{
		public MemberManagementDbContext(DbContextOptions<MemberManagementDbContext> options)
			: base(options) { }

		
		public DbSet<ManagerRoleEntry> ManagerRoles { get; set; }
		

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			
			
		}
	}
}

