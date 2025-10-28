# SQL Server 連線與讀取指引（MiniGame Area 專用版｜更新 2025-10-28）

## 更新記錄
- **2025-10-28**: 驗證連線配置穩定，確認所有20張表結構與索引正常運作
- **2025-10-27**: 更新資料庫名稱大小寫（GameSpacedatabase）、更新表格清單為實際20張表、新增寵物系統設定表格、修正連線字串格式、更新資料統計資訊
- **2025-10-16**: 初版建立

> 目的：提供其他 AI 代理依此文件即可完整重現 MiniGame Area 相關資料表（含權限管理）的連線、查詢與匯出流程。

## 1. 目標環境概覽
- **伺服器**：`DESKTOP-8HQIS1S\SQLEXPRESS`（強制使用 TCP 1433）
- **資料庫**：`GameSpacedatabase`（注意大小寫）
- **驗證模式**：Windows 整合認證（`-E`）或使用 Trusted_Connection
- **資料量**：20 個資料表（MiniGame Area 主要表格 16 張 + 使用者/權限相關表格 4 張）
- **SQL Server 版本**：Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 Express Edition
- **執行殼層**：Windows 11 Home（Build 26100）內建 PowerShell
- **主要工具**：`sqlcmd`（位於 `C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE`）
- **標準連線字串**：`Server=DESKTOP-8HQIS1S\SQLEXPRESS;Database=GameSpacedatabase;Trusted_Connection=True;TrustServerCertificate=True;`

## 2. 連線前檢查清單
1. **確認 `sqlcmd` 是否存在**  
   ```powershell
   Get-Command sqlcmd
   ```  
   若輸出類似 `Application SQLCMD.EXE` 則表示可用；若找不到需安裝 SQL Server Command Line Utilities。

2. **確定埠與協定**  
   必須指定 `tcp:` 前綴與 `,1433` 埠號，否則 `sqlcmd` 會預設使用命名管道導致連線失敗。

3. **作業目錄**  
   建議在 `GameSpace\schema` 目錄執行，方便將匯出檔案存放於同一處理路。

## 3. 標準連線流程（逐步）
以下流程為實際操作驗證過的指令組合（最後更新：2025-10-27）。

### 步驟 1：測試能否開啟連線
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E -Q "SELECT DB_NAME() AS CurrentDatabase"
```
- 預期輸出：一欄 `CurrentDatabase`，值為 `GameSpacedatabase`。
- 若出現 `Sqlcmd: Error` 且訊息提到語法錯誤，多半是引號轉義問題，請直接使用上述不含多餘引號的格式。

### 步驟 2：開啟互動式工作階段（必要時）
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E
```
- 輸入 `GO` 可執行緩衝內的 SQL。
- 使用 `EXIT` 結束互動式連線。

### PowerShell 引號陷阱
- 先前嘗試使用 `pwsh -Command "sqlcmd ..."` 會遇到 `*` 無法辨識或 `Sqlcmd: 'SELECT COUNT 1 ...'` 的錯誤，原因是 `pwsh` 會壓縮空白與引號。
- **建議**：直接執行 `sqlcmd` 指令（如上例），或在自動化腳本中改用參數陣列呼叫（`["sqlcmd","-S","tcp:..."]`）。

## 4. MiniGame Area 相關資料表速覽（共20張）

### 4.1 業務資料表（16張）

#### 錢包系統（2張）
- `User_Wallet` - 會員錢包（主鍵：User_Id）
- `WalletHistory` - 錢包異動歷史（主鍵：LogID）

#### 優惠券系統（2張）
- `Coupon` - 優惠券實例（主鍵：CouponID，唯一碼：CouponCode）
- `CouponType` - 優惠券類型定義（主鍵：CouponTypeID）

#### 電子禮券系統（4張）
- `EVoucher` - 電子禮券實例（主鍵：EVoucherID，唯一碼：EVoucherCode）
- `EVoucherType` - 電子禮券類型定義（主鍵：EVoucherTypeID）
- `EVoucherRedeemLog` - 電子禮券核銷記錄（主鍵：RedeemID）
- `EVoucherToken` - 電子禮券核銷憑證（主鍵：TokenID）

#### 簽到系統（2張）
- `UserSignInStats` - 簽到統計記錄（主鍵：LogID）
- `SignInRule` - 簽到規則設定（主鍵：Id）

#### 寵物系統（4張）
- `Pet` - 寵物系統資料（主鍵：PetID）
- `PetBackgroundCostSettings` - 寵物背景價格設定（主鍵：SettingId）
- `PetLevelRewardSettings` - 寵物升級獎勵規則（主鍵：SettingId）
- `PetSkinColorCostSettings` - 寵物膚色價格設定（主鍵：SettingId）

#### 遊戲記錄（1張）
- `MiniGame` - 小遊戲記錄（主鍵：PlayID）

#### 系統設定（1張）
- `SystemSettings` - 系統設定（主鍵：SettingId，唯一鍵：SettingKey）

### 4.2 權限與管理資料表（4張）
- `ManagerData` - 管理員基本資料（主鍵：Manager_Id）
- `ManagerRole` - 管理員角色分配（複合主鍵：Manager_Id, ManagerRole_Id）
- `ManagerRolePermission` - 角色權限定義（主鍵：ManagerRole_Id）
- `Users` - 使用者基本資料（主鍵：User_ID，關聯會員主檔）

> 提醒：權限分配流程需同時連動 `ManagerData → ManagerRole → ManagerRolePermission` 三張表，才能完整解析 RBAC 資訊。

### 4.3 重要設計特性
- **軟刪除機制**：所有 MiniGame Area 主要表格都實作軟刪除（IsDeleted, DeletedAt, DeletedBy, DeleteReason）
- **審計追蹤**：部分表格有 CreatedAt, UpdatedAt, UpdatedBy 欄位
- **時間戳記**：多數表格使用 `sysutcdatetime()` 自動填入 UTC 時間
- **資料完整性**：大量使用 CHECK Constraints 確保資料品質
- **唯一性約束**：優惠券代碼、禮券代碼、系統設定鍵值等都有唯一性限制

## 5. 常用查詢腳本

### 5.1 列出全部資料表（含模式）
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;SELECT TABLE_SCHEMA,TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_SCHEMA,TABLE_NAME"
```
- 操作結果：確認共有 20 張 `BASE TABLE`（MiniGame Area 相關表格）。

### 5.2 專注 MiniGame Area 的表清單
```powershell
$tables = \"'Coupon','CouponType','EVoucher','EVoucherType','EVoucherRedeemLog','EVoucherToken','MiniGame','Pet','PetBackgroundCostSettings','PetLevelRewardSettings','PetSkinColorCostSettings','SignInRule','SystemSettings','UserSignInStats','User_Wallet','WalletHistory','ManagerData','ManagerRole','ManagerRolePermission','Users'\"
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q \"SET NOCOUNT ON;SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ($tables) ORDER BY TABLE_NAME\"
```
- 可快速驗證 MiniGame Area 相關結構是否存在（共20張表）。

### 5.3 單表資料存取範例
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;SELECT TOP 10 User_Id, User_Point FROM dbo.User_Wallet ORDER BY User_Id"
```
- `SET NOCOUNT ON` 可避免額外的「(n 個資料列受到影響)」訊息，利於解析輸出。
- 注意：User_Wallet 表的主鍵是 `User_Id`（而非 User_Wallet_Id）。

## 6. 匯出 MiniGame Area 全量資料

### 6.1 匯出所有資料表（包含 MiniGame Area）
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;EXEC sp_MSforeachtable 'SELECT ''?'' AS TableName, * FROM ?'" `
  -o "GameSpacedatabase_all_tables.txt"
```
- 執行時間：約 2 秒（視資料量而定）。
- 輸出檔案位置：當前工作目錄（建議在 `work1\schema` 執行）
- 輸出檔案名稱：`GameSpacedatabase_all_tables.txt`
- 檔案格式：表名列 + 全欄位內容，適合離線審視或後續 AI 快速索引。

### 6.2 只匯出 MiniGame Area 相關資料表（20張）
```powershell
$minigameTables = @(
  'dbo.Coupon','dbo.CouponType',
  'dbo.EVoucher','dbo.EVoucherType','dbo.EVoucherRedeemLog','dbo.EVoucherToken',
  'dbo.MiniGame',
  'dbo.Pet','dbo.PetBackgroundCostSettings','dbo.PetLevelRewardSettings','dbo.PetSkinColorCostSettings',
  'dbo.SignInRule','dbo.SystemSettings',
  'dbo.UserSignInStats','dbo.User_Wallet','dbo.WalletHistory',
  'dbo.ManagerData','dbo.ManagerRole','dbo.ManagerRolePermission','dbo.Users'
)

# 逐表匯出範例
foreach ($table in $minigameTables) {
  $tableName = $table.Replace('dbo.', '')
  sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
    -Q "SET NOCOUNT ON;SELECT * FROM $table" `
    -o "${tableName}_export.txt"
}
```
- 若需客製匯出，可將上述 `$minigameTables` 調整為必要表清單；亦可改用 PowerShell 迴圈逐表呼叫 `bcp`。

## 7. 權限資料判讀指南
- **角色定義**：`ManagerRolePermission` 以欄位（如 `UserStatusManagement`, `Pet_Rights_Management`）記錄權限布林值，搭配 `role_name` 提供語意。
- **角色分配**：`ManagerRole` 連結 `Manager_Id` 與 `ManagerRole_Id`，判斷每位管理員擁有哪些角色。
- **管理員基本資料**：`ManagerData` 包含登入帳號、密碼雜湊、鎖定狀態等欄位，可與 `Users` 表的會員資料交叉比對。
- **實務查詢建議**：
  ```powershell
  sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
    -Q "SET NOCOUNT ON;
        SELECT md.Manager_Id, md.Manager_Name, mrp.role_name,
               mrp.UserStatusManagement, mrp.Pet_Rights_Management, mrp.ShoppingPermissionManagement
        FROM ManagerData md
        JOIN ManagerRole mr ON md.Manager_Id = mr.Manager_Id
        JOIN ManagerRolePermission mrp ON mr.ManagerRole_Id = mrp.ManagerRole_Id
        ORDER BY md.Manager_Id"
  ```

## 8. 常見錯誤與排除
- **`The term '*' is not recognized`**：PowerShell 將帶有 `*` 的查詢視為萬用字元；請改用不含 `*` 的 SQL、或以單引號包住整個查詢並使用 `-Q "..."`。
- **`Sqlcmd: 'SELECT COUNT 1 ...'`**：PowerShell 自動移除括號或空白。解法是直接以 `sqlcmd -S ... -Q "SELECT COUNT(1) FROM ..."` 執行，不要使用 `pwsh -Command` 另一層包裹。
- **`Login failed for user`**：確認目前的指令在 Windows 整合驗證下執行；若改以其他身分執行，需改傳 `-U <帳號> -P <密碼>`。
- **無檔案輸出**：確認 `-o` 指定的目錄可寫入，且指令完成後檔案大小非 0；必要時搭配 `Get-Item GameSpacedatabase_all_tables.txt | Format-List FullName,Length` 進行確認。

## 9. 連線字串格式參考

### 9.1 ADO.NET 連線字串（C# / .NET 應用程式）
```
Server=DESKTOP-8HQIS1S\SQLEXPRESS;Database=GameSpacedatabase;Trusted_Connection=True;TrustServerCertificate=True;
```

### 9.2 Entity Framework Core 連線字串
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-8HQIS1S\\SQLEXPRESS;Database=GameSpacedatabase;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 9.3 sqlcmd 連線參數
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E
```

## 10. 後續建議
1. 將此文件與資料庫結構摘要（`db_schema_summary.md`）一併納入版本控管，確保後續代理可快速回溯。
2. 若需長期自動化，建議撰寫 PowerShell 腳本封裝上述指令，並定期檢查 `sqlcmd` 與 SQL Server 的版本一致性。
3. MiniGame Area 內涉及會員敏感資訊，匯出資料請遵守原始專案的 RBAC 條件與使用範圍。
4. 定期更新此手冊，特別是新增表格或修改連線設定時。
