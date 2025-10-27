// 目的：當沒有透過 DI（Program.cs 的 AddDbContext）注入連線設定時，
//       由這個 partial 在執行期自行從 appsettings.json 讀取連線字串，讓 DbContext 仍可運作。

using System;                              // 需要 AppDomain.CurrentDomain.BaseDirectory
using Microsoft.EntityFrameworkCore;        // DbContext / DbContextOptionsBuilder / UseSqlServer
using Microsoft.Extensions.Configuration;   // ConfigurationBuilder / IConfigurationRoot

namespace GamiPort.Models
{
	public partial class GameSpacedatabaseContext : DbContext
	{
		// 無參數建構子：
		// - 允許在非 DI 場景（例如工具、測試、設計時）直接 new 這個 DbContext。
		// - 在 ASP.NET Core 正常執行時，優先建議由 DI 建構（搭配下面 OnConfiguring 的守門）。
		public GameSpacedatabaseContext() { }

		// 當 EF 建構此 DbContext 時會呼叫 OnConfiguring。
		// 我們只在「外部沒有設定過 options」的情況下，才啟用備援的 UseSqlServer 設定，
		// 以免覆蓋 Program.cs 的設定（例如不同連線字串、Log 選項等）。
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// IsConfigured == true 代表已由 DI（AddDbContext）配置過 options，
			// 此時不要再動設定，尊重 DI。
			if (!optionsBuilder.IsConfigured)
			{
				// 建立設定來源（只讀主設定檔 appsettings.json；不含環境覆寫檔）：
				// - BasePath 使用目前應用程式的輸出目錄（一般是 bin/Debug|Release/netX.Y/）
				// - 注意：若你採單檔發佈或特殊部署，請確保 appsettings.json 存在該目錄。
				IConfigurationRoot configuration = new ConfigurationBuilder()
					.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
					.Build();

				// 從 "ConnectionStrings" 區段取出名稱為 "GameSpace" 的連線字串。
				// - 若缺少該鍵，GetConnectionString 會回傳 null，UseSqlServer 會在執行時出錯。
				// - 正式環境建議優先用 DI 配置（支援環境變數/Secret/環境覆寫）。
				var conn = configuration.GetConnectionString("GameSpace");

				// 套用 SQL Server 提供者與連線字串。
				// 這裡只會在「沒有 DI 設定」時生效，作為備援。
				optionsBuilder.UseSqlServer(conn);
			}
		}
	}
}
