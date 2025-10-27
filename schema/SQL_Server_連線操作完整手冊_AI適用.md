# SQL Server 連線與讀取指引（MiniGame Area 專用版｜更新 2025-10-16）

> 目的：提供其他 AI 代理依此文件即可完整重現 MiniGame Area 相關資料表（含權限管理）的連線、查詢與匯出流程。

## 1. 目標環境概覽
- **伺服器**：`DESKTOP-8HQIS1S\SQLEXPRESS`（強制使用 TCP 1433）
- **資料庫**：`GameSpacedatabase`
- **驗證模式**：Windows 整合認證（`-E`）
- **資料量**：88 個資料表、約 17,063 筆資料列
- **執行殼層**：Windows 11 Home（Build 26100）內建 PowerShell
- **主要工具**：`sqlcmd`（位於 `C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE`）

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
以下流程為 2025/10/16 實際操作驗證過的指令組合。

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

## 4. MiniGame Area 相關資料表速覽

### 4.1 業務資料表
- `User_Wallet`、`WalletHistory`
- `Coupon`、`CouponType`
- `EVoucher`、`EVoucherType`、`EVoucherRedeemLog`、`EVoucherToken`
- `UserSignInStats`、`SignInRule`
- `Pet`
- `MiniGame`

### 4.2 權限與管理資料表
- `ManagerData`
- `ManagerRole`
- `ManagerRolePermission`
- `Users`（關聯會員主檔）

> 提醒：權限分配流程需同時連動 `ManagerData → ManagerRole → ManagerRolePermission` 三張表，才能完整解析 RBAC 資訊。

## 5. 常用查詢腳本

### 5.1 列出全部資料表（含模式）
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;SELECT TABLE_SCHEMA,TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_SCHEMA,TABLE_NAME"
```
- 操作結果：確認共有 88 張 `BASE TABLE`。

### 5.2 專注 MiniGame Area 的表清單
```powershell
$tables = \"'Coupon','CouponType','EVoucher','EVoucherType','EVoucherRedeemLog','EVoucherToken','MiniGame','Pet','SignInRule','UserSignInStats','User_Wallet','WalletHistory','ManagerData','ManagerRole','ManagerRolePermission','Users'\"
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q \"SET NOCOUNT ON;SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ($tables) ORDER BY TABLE_NAME\"
```
- 可快速驗證 MiniGame Area 相關結構是否存在。

### 5.3 單表資料存取範例
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;SELECT TOP 10 User_Wallet_Id, User_Id, User_Point FROM dbo.User_Wallet ORDER BY User_Wallet_Id"
```
- `SET NOCOUNT ON` 可避免額外的「(n 個資料列受到影響)」訊息，利於解析輸出。

## 6. 匯出 MiniGame Area 全量資料

### 6.1 匯出所有資料表（包含 MiniGame Area）
```powershell
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;EXEC sp_MSforeachtable 'SELECT ''?'' AS TableName, * FROM ?'" `
  -o "GameSpacedatabase_all_tables.txt"
```
- 執行時間：約 2 秒。
- 輸出檔案位置：`GameSpace\schema\GameSpacedatabase_all_tables.txt`
- 檔案大小：約 5.07 MB（5,321,326 bytes）
- 檔案格式：表名列 + 全欄位內容，適合離線審視或後續 AI 快速索引。

### 6.2 只匯出 MiniGame Area 相關資料表
```powershell
$minigameTables = @(
  'dbo.Coupon','dbo.CouponType','dbo.EVoucher','dbo.EVoucherType','dbo.EVoucherRedeemLog','dbo.EVoucherToken',
  'dbo.MiniGame','dbo.Pet','dbo.SignInRule','dbo.UserSignInStats','dbo.User_Wallet','dbo.WalletHistory',
  'dbo.ManagerData','dbo.ManagerRole','dbo.ManagerRolePermission','dbo.Users'
)
$script = $minigameTables -join ','
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E `
  -Q "SET NOCOUNT ON;EXEC sp_executesql N'SELECT ''?'' AS TableName, * FROM ?' ,N'@list NVARCHAR(MAX)', @list=N'$script'"
```
- 若需客製匯出，可將上述 `$minigameTables` 調整為必要表清單，再包裝為 `FOREACH` 執行；亦可改用 PowerShell 迴圈逐表呼叫 `bcp`。

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

## 9. 後續建議
1. 將此文件與 `GameSpacedatabase_all_tables.txt` 一併納入版本控管，確保後續代理可快速回溯。
2. 若需長期自動化，建議撰寫 PowerShell 腳本封裝上述指令，並定期檢查 `sqlcmd` 與 SQL Server 的版本一致性。
3. MiniGame Area 內涉及會員敏感資訊，匯出資料請遵守原始專案的 RBAC 條件與使用範圍。
