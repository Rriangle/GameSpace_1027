# GameSpace – MiniGame Area（Admin 專注）整合規格（README\_合併版）

> **最後更新日期：2025-10-27**
> 說明語言：**zh-TW**（程式識別符、檔名、路徑、SQL/CLI 關鍵字不可翻譯）
> 編碼：**UTF-8 with BOM**
> 本檔為你提供：**單一文件就能讓 AI/開發者理解整個 MiniGame Area 的後台規格、資料庫覆蓋清單、權限/登入整合、檔案落點與驗收清單**。
> **請一律用 powershell 寫檔案**

---

## 0. 來源與權威層級（Single Source of Truth）

- **90% 規格**：`專案規格敘述1.txt` + `專案規格敘述2.txt`
- **+10% 補齊**：`這裡有整個專案各個Area的資料庫結構及種子資料(有些種子資料尚未填入).sql`

  - 此檔為**完整的資料庫結構與種子資料**（部分種子仍待補），對前兩份文字規格無法涵蓋的 10% 進行**合理推斷**與補齊。
  - **MiniGame Area 與管理者權限**相關的結構與種子資料以此 SQL 為準。

- **管理者權限文字說明**：`管理者權限相關描述.txt`
- **MiniGame Area 專用補充**：`MiniGame_Area_完整描述文件.md`、`MiniGame_area功能彙整.txt`
- **前台視覺與互動**：`index.txt`（僅供 Public 前台樣式；目前本組**只做 Admin**）

> **重要**：**資料庫唯一真相源**為 `schema` 夾內的資料庫（供 AI 閱讀）；實際執行以 **本機 SQL Server（SSMS 已建立）**為準，**嚴禁 EF Migrations 修改 schema**。

> 備註：本文件不主張任何未經確認的檔名/路徑；具體實作請以實際專案為準（或於稽核後再補充）。

---

## 1. 專案工作分工與界線（本組規範）

- 本組目前 **只專心開發 Admin 後台（含 Admin 前端樣式與 Admin 後端 API）**。
- 我（本文件負責人）被分配到 **`Areas/MiniGame` 的後端**，**只能在 `Areas/MiniGame/**` 下作業\*\*。
  - **嚴格禁止**越界修改他人 Area 或全域檔案。
  - **唯一例外**：`Program.cs` 可**僅新增必要註冊碼**來註冊 MiniGame Area（不得動他人註冊與設定）。
- Admin 首頁與樣式為**統一樣板（SB Admin）不可更動**；由首頁點選 **MiniGame 標籤 → 展開 MiniGame 旗下的各模組子標籤**，進入各後台功能頁。

**UI 樣式參考：**

- **Public（前台）**：遵照 `index.txt`（Bootstrap-based）。
- **Admin（後台）**：**SB Admin**（`wwwroot/lib/sb-admin/` 供應；**禁止**修改 vendor 檔）。

---

## 2. 登入、Cookie 與權限（與「共用登入」Area 串接）

- **登入重導向**：`Areas/MiniGame/Controllers/HomeController.cs` 的 `Login()` **不得自建登入流程**，應**正確重導向**到**主登入系統**（主登入由專案的登入模組提供；實際檔名/路徑以專案為準）。

  ```csharp
  // Areas/MiniGame/Controllers/HomeController.cs（範例）
  [AllowAnonymous]
  public IActionResult Login(string? returnUrl = null)
      => RedirectToAction("Index", "Login", new {
          area = "", // 主登入在根層
          returnUrl = Url.Action("Index", "Home", new { area = "MiniGame" })
      });
  ```

- **統一 Cookie 驗證**：**主登入系統與 MiniGame Area 均使用 `AdminCookie` scheme**。

  - （若專案已有登入相容機制，例如會先讀取 `AdminCookie` 或相容 `gs_*`，請沿用；若無，請依下列策略設定。）
  - `Program.cs` **需註冊**（示意）：
    ```csharp
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "AdminCookie";
        options.DefaultChallengeScheme = "AdminCookie";
    })
    .AddCookie("AdminCookie", o =>
    {
        o.LoginPath = "/Login/Index";
        o.AccessDeniedPath = "/Login/AccessDenied";
        o.SlidingExpiration = true;
    });
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", p => p.RequireClaim("IsManager", "true"));
        // 若需細分 MiniGame 權限，可用 RequireClaim("CanMiniGameAdmin","true")
    });
    app.UseAuthentication();
    app.UseAuthorization();
    ```

- **Claims 與 Manager 權限**：登入時會設定正確的 **Claims** 與權限（例如：`IsManager=true`、角色/功能點）。

  - 請**串接 Manager 權限表**（例如：`ManagerData`、`ManagerRole`、`ManagerRolePermission` 等）以核對權限。
  - 可在 Controller / Razor 加上：`[Authorize(AuthenticationSchemes="AdminCookie", Policy="AdminOnly")]`。

- **側邊欄導航**：在 **共用 `_Sidebar.cshtml`** 中已有 **「小遊戲」**連結指向 `asp-area="MiniGame"`。
  - MiniGame Area 自有 `_Sidebar.cshtml`：第一層為模組、第二層為**本文件下文列出的固定按鈕名稱**。

---

## 3. MiniGame Admin 導覽與功能矩陣（**兩層 Sidebar**，第二層按鈕固定如下）

> **注意**：下列 **Admin** 第二層按鈕**名稱必須與括號內對應之功能一致**（**不多不少，不可腦補**）。Client（前台）只作為系統藍圖描述，**本組本期不實作**。

### 3.1 會員錢包

- **Client（僅描述，不實作）**

  1. **查看當前會員點數餘額**
  2. **使用會員點數兌換商城優惠券及電子優惠券**
  3. **查看目前擁有商城優惠券**
  4. **查看目前擁有電子優惠券**
  5. **使用電子優惠券**（以 **QRCode/Barcode** 顯示予店員核銷）
  6. **查看收支明細**（點數得到/花費、商城優惠券得到/使用、電子優惠券得到/使用之**時間/點數/張數/種類**…）

- **Admin（本期必做，作為第二層按鈕）**
  1. **查詢會員點數**
  2. **查詢會員擁有商城優惠券**
  3. **查詢會員擁有電子禮券**
  4. **發放會員點數**
  5. **發放會員擁有商城優惠券**（含發放）
  6. **調整會員擁有電子禮券**（發放）
  7. **查看會員收支明細**（點數得到/花費、商城優惠券得到/使用、電子優惠券得到/使用之**時間/點數/張數/種類**…）

### 3.2 會員簽到系統

- **Client（僅描述，不實作）**

  1. **查看月曆型簽到簿並執行簽到**
  2. **查看簽到歷史紀錄**（何時簽到與獎品：點數/寵物經驗/商城優惠券）

- **Admin（本期必做，第二層按鈕）**
  1. **簽到規則設定**
  2. **查看會員簽到紀錄**

### 3.3 寵物系統

- **Client（僅描述，不實作）**

  1. **寵物名字修改**
  2. **寵物互動**（餵食/洗澡/玩耍/哄睡）
  3. **寵物換膚色**（扣會員點數）
  4. **寵物換背景**（可免費或需點數）

- **Admin（本期必做，第二層按鈕）**
  1. **整體寵物系統規則設定**（升級規則/互動增益/可選膚色與所需點數/可選背景與所需點數）
  2. **會員個別寵物設定**手動調整基本資料（寵物名、膚色、背景）
  3. **會員個別寵物清單含查詢**（寵物名/膚色/背景/經驗/等級/五大狀態）＋ **換膚／換背景紀錄查詢**

### 3.4 小遊戲系統

- **Client（僅描述，不實作）**

  1. **出發冒險**：啟動遊戲流程，回傳 `sessionId`、`startTime`、預估結束時間、**當日剩餘可玩次數**。
  2. **查看遊戲紀錄**：每場 `startTime`/`endTime`/`result(win|lose|abort)`/獎勵（點數/寵物經驗/商城優惠券）。

- **Admin（本期必做，第二層按鈕）**
  1. **遊戲規則設定**（獎勵規則、**每日遊戲次數限制（預設 3 次/日）**）
  2. **查看會員遊戲紀錄**（`startTime`、`endTime`、`win/lose/abort`、獲得獎勵）

---

## 4. 後端落點與控制器建議（不更動他人 Area）

- **只能建立/修改下列路徑**：`Areas/MiniGame/Controllers|Models|Services|Views/**`
- **建議控制器（僅命名建議，對應 3.x Admin 按鈕）**：
  - `AdminWalletController`：查詢/發放點數、券類、收支明細
  - `AdminSignInRulesController`、`AdminSignInStatsController`
  - `AdminPetRulesController`、`AdminPetController`
  - `AdminMiniGameRulesController`、`AdminMiniGameRecordsController`
- **認證與授權層**：上述控制器/頁面一律 `[Authorize(AuthenticationSchemes="AdminCookie", Policy="AdminOnly")]`。

---

## 5. 資料庫覆蓋要求（MiniGame 相關表 **100% COVER**）

> 讀取**既有本機 SQL Server**（SSMS 已建與灑 seed），`schema/` 內 SQL/JSON 僅供 AI 參考。**嚴禁**修改 schema、**不使用** EF Migrations。

**必覆蓋（讀/寫依功能而定）**（以實際資料庫結構為準）：

- **錢包/券類**：`User_Wallet`（僅含 User_Id、User_Point、軟刪除欄位）、`WalletHistory`（含 ChangeType、PointsChanged、ItemCode、Description、ChangeTime）、`CouponType`（含 Name、DiscountType、DiscountValue、MinSpend、ValidFrom、ValidTo、PointsCost）、`Coupon`（含 CouponCode、IsUsed、AcquiredTime、UsedTime、UsedInOrderID）、`EVoucherType`（含 Name、ValueAmount、ValidFrom、ValidTo、PointsCost、TotalAvailable）、`EVoucher`（含 EVoucherCode、IsUsed、AcquiredTime、UsedTime）、`EVoucherToken`（含 Token、ExpiresAt、IsRevoked）、`EVoucherRedeemLog`（含 Status: REVOKED/REJECTED/EXPIRED/ALREADYUSED/APPROVED）
- **簽到**：`UserSignInStats`（含 SignTime、PointsGained、PointsGainedTime、ExpGained、ExpGainedTime、CouponGained、CouponGainedTime）、`SignInRule`（含 SignInDay、Points、Experience、HasCoupon、CouponTypeCode、IsActive）
- **寵物**：`Pet`（含五大屬性：Hunger/Mood/Stamina/Cleanliness/Health 範圍 0-100、Level、Experience、CurrentExperience、ExperienceToNextLevel、SkinColor(varchar(7) #RRGGBB)、SkinColorChangedTime、BackgroundColor(nvarchar(20))、BackgroundColorChangedTime、PointsChanged_SkinColor、PointsChanged_BackgroundColor、PointsGained_LevelUp、PointsGainedTime_LevelUp、TotalPointsGained_LevelUp）、`PetSkinColorCostSettings`（含 ColorCode、ColorName、PointsCost、Rarity、IsFree、IsLimitedEdition）、`PetBackgroundCostSettings`（含 BackgroundCode、BackgroundName、PointsCost）、`PetLevelRewardSettings`（含 LevelRangeStart、LevelRangeEnd、PointsReward）
- **小遊戲**：`MiniGame`（含 Level、MonsterCount、SpeedMultiplier、Result、ExpGained、ExpGainedTime、PointsGained、PointsGainedTime、CouponGained、CouponGainedTime、HungerDelta、MoodDelta、StaminaDelta、CleanlinessDelta、StartTime、EndTime、Aborted）
- **管理者/權限**：`ManagerData`（含 Manager_Email、Manager_EmailConfirmed、Manager_AccessFailedCount、Manager_LockoutEnabled、Manager_LockoutEnd）、`ManagerRole`、`ManagerRolePermission`（含 role_name、AdministratorPrivilegesManagement、UserStatusManagement、ShoppingPermissionManagement、MessagePermissionManagement、Pet_Rights_Management、customer_service）
- **系統設定**：`SystemSettings`（含 SettingKey、SettingValue、Category、SettingType: String/Boolean/Number/JSON、IsReadOnly、IsActive）

**約束必守**：`PK`/`FK`/`UNIQUE`/`CHECK`/`DEFAULT` 全通過；**Idempotent** 寫入、批量上限 **≤ 1000**。

---

## 6. 與前台/樣式的關係（僅參考，不混用資產）

- Public（前台）樣式＝`index.txt`（Bootstrap）。
- Admin（後台）樣式＝**SB Admin**；**禁止**混用或修改 vendor：  
  `wwwroot/lib/sb-admin/`、`wwwroot/lib/bootstrap/`、`wwwroot/lib/font-awesome/`。

---

- **404 Not Found**：全專案已有**共用 404 頁**（由共用層提供）；**MiniGame Area 不得另建** 404 頁面。

## 7. 程式設定與連線

- **appsettings.json**：設定 `DefaultConnection` 指向**本機 SQL Server**。
- **Program.cs**：僅新增 **MiniGame Area** 所需註冊（Authentication/Authorization/Route Map）；不得動他人設定。
  ```csharp
  app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
  ```
- **健康檢查**：`/healthz/db` 回傳 `{"status":"ok"}` 或錯誤 JSON（用來驗證 DB 連線與查詢範例）。

---

## 8. 稽核與修復（MiniGame Area 專屬）

請**全面稽核並修復** MiniGame Area 是否存在：

1. **未符合或未實作** schema / 規格、**錯誤命名**、**未正確引用**、**缺少 Models/Controllers/Views/Services**、**無用檔案**。
2. **中文編碼錯誤**（請全案以 **UTF-8 with BOM** 儲存）。
3. **MiniGame 相關表格/欄位**是否**100% COVER**（含查詢、發放、紀錄查詢、規則設定等後台功能）。
4. **Manager 權限判斷**是否落實（`AdminCookie`、`Claims` 與 Manager 表串接）。
5. **實際可連上 SQL Server**，並能**讀取既有種子假資料**（顯示至各 Admin 查詢頁）。
6. **MiniGame `_Sidebar.cshtml`** 第二層按鈕是否**完全等同**第 3 章所列名稱。

**修復流程（Drift Repair）**：

- 停止擴充範圍 → 指認偏移 → 修正以**符合本檔 + 舊新規格 + schema** →
- Commit message 記錄：**WHY drifted / HOW corrected** → 再繼續。

---

## 9. 交付邊界（本期）

- **只做 Admin 後台（Server 功能）**，對應第 3 章 **Admin** 清單。
- Client（前台）功能先不實作，但保留資料讀寫介面上的**一致性**與**命名**，以利未來擴展。

---

## 10. 驗收清單（Smoke / 功能）

- [ ] 使用 `AdminCookie` 登入後可進入 `/MiniGame/Home/Index`。
- [ ] 共用 `_Sidebar.cshtml` 含 **小遊戲**連結，MiniGame `_Sidebar.cshtml` 之**第二層按鈕**與第 3 章**逐字一致**。
- [ ] **會員錢包（Admin）**：可查詢/發放點數、查詢/發放券、查閱收支明細。
- [ ] **簽到（Admin）**：可設定規則、查閱會員簽到紀錄。
- [ ] **寵物（Admin）**：可設定全域規則、調整個別寵物資料、查詢清單與變更紀錄。
- [ ] **小遊戲（Admin）**：可設定獎勵規則與每日次數限制、查閱會員遊戲紀錄。
- [ ] **/healthz/db** 成功回傳 `{"status":"ok"}`。
- [ ] 所有檔案 **UTF-8 with BOM**。

---

## 12. 開發注意事項（Coding Style / 性能 / 安全）

- Read-only 查詢使用 `AsNoTracking()`；傳回 **Read Model/DTO**。
- 交易（扣點/發券/簽到結算/小遊戲結算）必在 **Transaction** 中進行，避免負餘額與並發問題。
- 日誌：**Serilog** + CorrelationId；敏感操作記錄審計。
- 錯誤回應使用 `ProblemDetails` 或統一 Result 型別。
- **禁止**一次性巨量編輯；每次提交 **≤ 3 檔 / ≤ 400 行**（與 CI 規範一致）。

---

## 13. 手動 DB 初始化與本機執行（Manual）

- 入口：提供**可呼叫的 Seeder/Runner**（**不**自動於啟動）。
- Healthcheck：`/healthz/db` 回 `{status:'ok'}` 或錯誤細節。
- Seeding：**Idempotent**、批次 **≤ 1000**、記錄英文 key。
- README 段落：**Manual DB initialization & local run**（本檔即為之）。

本機步驟（摘要）：

1. 安裝 VS 2022+ 與 SQL Server 2019/2022。
2. 在 SSMS 執行 SQL（建立 **GameSpaceDatabase**）。
3. 執行 seed（含 **MiniGame Area** 展示用假資料；**不得違反 schema**）。
4. 開啟 ASP.NET Core MVC 解決方案；設定 `appsettings.json` 的 `DefaultConnection`。
5. 建置並執行（Admin 站台）；以 **AdminCookie** 登入後進入 MiniGame 後台。

---

## 14. 變更紀錄

- 2025-09-25：**整合新版規格**（Admin-only、`AdminCookie`、Login 重導、第二層按鈕定名）、補齊 DB 覆蓋與稽核流程、UTF-8 with BOM 要求。
