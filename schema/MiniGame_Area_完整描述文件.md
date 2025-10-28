# GameSpace MiniGame Area 完整描述文件

## 更新記錄

**2025-10-27 更新** - 根據實際資料庫結構更新所有技術細節
- 更新所有資料表欄位定義與實際資料庫一致
- 更新約束、索引、外鍵定義
- 更新資料型別與預設值
- 保持業務邏輯與功能描述不變

---

## 文件概述

本文件基於以下權威資料來源彙整而成：

- README\_合併版.md- 專案開發規範與流程 -**90% 規格**：`專案規格敘述1.txt` + `專案規格敘述2.txt`
- **+10% 補齊**：`這裡有整個專案各個Area的資料庫結構及種子資料(有些種子資料尚未填入).sql`

  - 此檔為**完整的資料庫結構與種子資料**（部分種子仍待補），對前兩份文字規格無法涵蓋的 10% 進行**合理推斷**與補齊。
  - **MiniGame Area 與管理者權限**相關的結構與種子資料以此 SQL 為準。

- **管理者權限文字說明**：`管理者權限相關描述.txt`
- **MiniGame Area 專用補充**：`MiniGame_Area_完整描述文件.md`、`MiniGame_area功能彙整.txt`
- **前台視覺與互動**：`index.txt`（僅供 Public 前台樣式；目前本組**只做 Admin**）

> **重要**：**資料庫唯一真相源**為 `schema` 夾內的資料庫（供 AI 閱讀）；實際執行以 **本機 SQL Server（SSMS 已建立）**為準，**嚴禁 EF Migrations 修改 schema**。

## 1. MiniGame Area 系統概述

### 1.1 系統定位

MiniGame Area 是 GameSpace 遊戲論壇平台的核心功能區域，提供會員點數管理、每日簽到、寵物養成、小遊戲冒險等互動功能。該區域採用 ASP.NET MVC Areas 架構，確保模組化開發與維護。

### 1.2 技術架構

- **後端框架**: ASP.NET Core MVC + C#
- **資料庫**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **前端技術**: Razor 模板引擎 + Bootstrap + jQuery + Vue.js
- **架構模式**: 三層式架構 (Presentation/Business/Data Layer)
- **權限控制**: 基於角色的存取控制 (RBAC)

### 1.3 模組組成

根據 contributing_agent.yaml 定義，MiniGame Area 包含四個核心模組：

1. **User_Wallet** - 會員點數系統
2. **UserSignInStats** - 每日簽到系統
3. **Pet** - 寵物養成系統
4. **MiniGame** - 小遊戲冒險系統

## 2. 資料庫結構對應

### 2.1 核心資料表

基於 database.json 權威定義，MiniGame Area 涉及以下資料表：

#### User_Wallet 模組相關表

##### User_Wallet - 會員錢包主檔
**欄位**:
- User_Id (int, NOT NULL, PK & FK → Users.User_ID)
- User_Point (int, NOT NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

##### CouponType - 優惠券類型定義
**欄位**:
- CouponTypeID (int, IDENTITY, PK)
- Name (nvarchar(50), NOT NULL, UNIQUE)
- DiscountType (nvarchar(20), NOT NULL) - 'PERCENT' 或 'AMOUNT'
- DiscountValue (decimal(18,2), NULL)
- MinSpend (decimal(18,2), NULL)
- ValidFrom (datetime2(7), NOT NULL)
- ValidTo (datetime2(7), NOT NULL)
- PointsCost (int, NOT NULL)
- Description (nvarchar(600), NULL)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**約束**:
- CK_CouponType_DiscountType: DiscountType 必須是 'PERCENT' 或 'AMOUNT' (大寫)
- CK_CouponType_ValidRange: ValidFrom <= ValidTo

##### Coupon - 優惠券實例
詳見 3.3 節

##### EVoucherType - 電子禮券類型定義
**欄位**:
- EVoucherTypeID (int, IDENTITY, PK)
- Name (nvarchar(50), NOT NULL)
- ValueAmount (decimal(18,2), NOT NULL)
- ValidFrom (datetime2(7), NOT NULL)
- ValidTo (datetime2(7), NOT NULL)
- PointsCost (int, NOT NULL)
- TotalAvailable (int, NOT NULL)
- Description (nvarchar(600), NULL)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

##### EVoucher - 電子禮券實例
詳見 3.2 節

##### EVoucherToken - 禮券核銷憑證
詳見 3.2.2 節

##### EVoucherRedeemLog - 禮券核銷記錄
詳見 3.2.3 節

##### WalletHistory - 錢包異動歷史
詳見 3.4 節

#### UserSignInStats 模組相關表

##### UserSignInStats - 簽到統計記錄
**欄位**:
- LogID (int, IDENTITY, PK)
- SignTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UserID (int, NOT NULL, FK → Users.User_ID)
- PointsGained (int, NOT NULL)
- PointsGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- ExpGained (int, NOT NULL)
- ExpGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- CouponGained (nvarchar(50), NOT NULL)
- CouponGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

##### SignInRule - 簽到規則設定
**欄位**:
- Id (int, IDENTITY, PK)
- SignInDay (int, NOT NULL, UNIQUE) - 範圍 1-365
- Points (int, NOT NULL) - >= 0
- Experience (int, NOT NULL) - >= 0
- HasCoupon (bit, NOT NULL, DEFAULT 0)
- CouponTypeCode (nvarchar(50), NULL)
- IsActive (bit, NOT NULL, DEFAULT 1)
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- Description (nvarchar(255), NULL)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**約束**:
- CK_SignInRule_DayRange: SignInDay 必須在 1-365 之間
- CK_SignInRule_Positive: Points 和 Experience 必須 >= 0
- CK_SignInRule_CouponFlag: HasCoupon=1 時 CouponTypeCode 必須有值；HasCoupon=0 時必須為 NULL

**索引**:
- UQ_SignInRule_SignInDay_Active (UNIQUE) on SignInDay
- IX_SignInRule_IsDeleted on IsDeleted

#### Pet 模組相關表

##### Pet - 寵物狀態資料
詳見第 4.3 節

#### MiniGame 模組相關表

##### MiniGame - 小遊戲記錄
詳見第 4.4 節

#### 系統設定表

##### SystemSettings - 系統設定
**欄位**:
- SettingId (int, IDENTITY, PK)
- SettingKey (nvarchar(200), NOT NULL, UNIQUE)
- SettingValue (nvarchar(MAX), NULL)
- Description (nvarchar(500), NULL)
- Category (nvarchar(100), NOT NULL, DEFAULT 'General')
- SettingType (nvarchar(50), NOT NULL, DEFAULT 'String') - 可選值: 'String', 'Boolean', 'Number', 'JSON'
- IsReadOnly (bit, NOT NULL, DEFAULT 0)
- IsActive (bit, NOT NULL, DEFAULT 1)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- UpdatedBy (int, NULL)

**CHECK Constraints**:
- CHK_SystemSettings_SettingType: SettingType IN ('String', 'Boolean', 'Number', 'JSON')

**索引**:
- UQ_SystemSettings_SettingKey (UNIQUE) on SettingKey

### 2.2 權限管理表

基於 script_manager.sql 和 database.json 權威定義：

#### 管理員資料表

##### ManagerData - 管理員基本資料
**主鍵**: Manager_Id (int)

**欄位**:
- Manager_Id (int, NOT NULL) - PK
- Manager_Name (nvarchar(30), NULL) - 管理員姓名
- Manager_Account (varchar(30), NULL) - 管理員帳號
- Manager_Password (nvarchar(200), NULL) - 管理員密碼
- Administrator_registration_date (datetime2(7), NULL) - 註冊日期
- Manager_Email (nvarchar(255), NOT NULL) - 管理員信箱
- Manager_EmailConfirmed (bit, NOT NULL, DEFAULT 0) - 信箱確認狀態
- Manager_AccessFailedCount (int, NOT NULL, DEFAULT 0) - 登入失敗次數
- Manager_LockoutEnabled (bit, NOT NULL, DEFAULT 0) - 帳號鎖定啟用
- Manager_LockoutEnd (datetime2(7), NULL) - 帳號鎖定結束時間

#### 角色權限表

##### ManagerRolePermission - 角色權限定義
**主鍵**: ManagerRole_Id (int)

**欄位**:
- ManagerRole_Id (int, NOT NULL) - PK
- role_name (nvarchar(50), NOT NULL) - 角色名稱
- AdministratorPrivilegesManagement (bit, NULL, DEFAULT NULL) - 管理者平台管理權限
- UserStatusManagement (bit, NULL, DEFAULT NULL) - 使用者狀態管理權限
- ShoppingPermissionManagement (bit, NULL, DEFAULT NULL) - 購物權限管理
- MessagePermissionManagement (bit, NULL, DEFAULT NULL) - 訊息權限管理
- Pet_Rights_Management (bit, NULL, DEFAULT NULL) - 寵物權限管理
- customer_service (bit, NULL, DEFAULT NULL) - 客服權限

#### 管理員角色分配表

##### ManagerRole - 管理員角色分配
**主鍵**: 複合主鍵 (Manager_Id, ManagerRole_Id)
**外鍵**:
- Manager_Id → ManagerData.Manager_Id (NO_ACTION)
- ManagerRole_Id → ManagerRolePermission.ManagerRole_Id (NO_ACTION)

**欄位**:
- Manager_Id (int, NOT NULL) - PK & FK
- ManagerRole_Id (int, NOT NULL) - PK & FK

### 2.3 使用者資料表

##### Users - 使用者基本資料
**主鍵**: User_ID (int, IDENTITY)

**欄位**:
- User_ID (int, NOT NULL, IDENTITY) - PK
- User_name (nvarchar(30), NOT NULL) - 使用者名稱
- User_Account (nvarchar(30), NOT NULL) - 使用者帳號
- User_Password (nvarchar(255), NOT NULL) - 使用者密碼
- User_EmailConfirmed (bit, NOT NULL, DEFAULT 0) - 信箱確認狀態
- User_PhoneNumberConfirmed (bit, NOT NULL, DEFAULT 0) - 手機號碼確認狀態
- User_TwoFactorEnabled (bit, NOT NULL, DEFAULT 0) - 雙因素驗證啟用
- User_AccessFailedCount (int, NOT NULL, DEFAULT 0) - 登入失敗次數
- User_LockoutEnabled (bit, NOT NULL, DEFAULT 0) - 帳號鎖定啟用
- User_LockoutEnd (datetime2(7), NULL) - 帳號鎖定結束時間
- Create_Account (datetime2(6), NOT NULL) - 帳號建立時間

## 3. 會員錢包系統詳細分析

### 3.1 會員錢包系統組成

根據 seedMiniGameArea.json 的假資料分析，會員錢包系統包含三種金流相關：

#### 3.1.1 會員點數 (Point)

- **儲存位置**: User_Wallet 表的 User_Point 欄位
- **用途**: 兌換優惠券、禮券、購買寵物膚色/背景
- **異動記錄**: WalletHistory 表記錄所有點數變動

#### 3.1.2 商城優惠券 (Coupon)

- **儲存位置**: Coupon 表
- **類型定義**: CouponType 表定義不同優惠券類型
- **序號格式**: CPN-YYYYMM-XXXXXX (例如: CPN-2508-AAG049)
- **使用範圍**: 僅限官方商城內使用
- **狀態管理**: IsUsed (0=未使用, 1=已使用), UsedTime, UsedInOrderID

#### 3.1.3 電子禮券 (EVoucher)

- **儲存位置**: EVoucher 表
- **類型定義**: EVoucherType 表定義不同禮券類型
- **序號格式**: EV-{類型}-{隨機碼}-{6 位數字} (例如: EV-MOVIE-8JDW-064877)
- **使用範圍**: 實體店核銷使用
- **核銷機制**: 透過 EVoucherToken 和 EVoucherRedeemLog 管理

**EVoucher 表結構**:
- EVoucherID (int, IDENTITY, PK)
- EVoucherCode (nvarchar(50), NOT NULL, UNIQUE)
- EVoucherTypeID (int, NOT NULL, FK → EVoucherType.EVoucherTypeID)
- UserID (int, NOT NULL, FK → Users.User_ID)
- IsUsed (bit, NOT NULL, DEFAULT 0)
- AcquiredTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UsedTime (datetime2(7), NULL, DEFAULT sysutcdatetime())
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**索引**:
- UQ_EVoucher_EVoucherCode (UNIQUE) on EVoucherCode
- IX_EVoucher_IsDeleted on IsDeleted
- IX_EVoucher_user_used on (UserID, IsUsed, AcquiredTime)

### 3.2 電子禮券核銷系統詳細分析

#### 3.2.1 序號生成規則

根據 seedMiniGameArea.json 分析，電子禮券序號格式為：
`EV-{類型代碼}-{4位隨機碼}-{6位數字}`

**類型代碼對應**:

- CASH - 現金禮券
- MOVIE - 電影票券
- FOOD - 餐飲券
- GAS - 加油券
- STORE - 商店券
- COFFEE - 咖啡券

**範例序號**:

- EV-MOVIE-8JDW-064877 - 電影票券
- EV-CASH-VR2G-969669 - 現金禮券
- EV-FOOD-DKTG-417690 - 餐飲券
- EV-GAS-KELK-380122 - 加油券

#### 3.2.2 核銷憑證系統 (EVoucherToken)

`json
{
  "TokenID": 1,
  "EVoucherID": 1,
  "Token": "TKN-GFWZIUGU-7603",
  "ExpiresAt": "2023-07-15 03:39:23",
  "IsRevoked": 0,
  "IsDeleted": 0,
  "DeletedAt": null,
  "DeletedBy": null,
  "DeleteReason": null
}
`

**欄位說明**:

- **TokenID**: 憑證唯一識別碼 (int, IDENTITY, PK)
- **EVoucherID**: 對應的電子禮券 ID (int, NOT NULL, FK → EVoucher.EVoucherID)
- **Token**: 核銷憑證碼 (varchar(64), NOT NULL)，格式 TKN-{8 位隨機碼}-{4 位數字}
- **ExpiresAt**: 憑證過期時間 (datetime2(7), NOT NULL)
- **IsRevoked**: 是否已撤銷 (bit, NOT NULL, DEFAULT 0) - 0=有效, 1=已撤銷
- **IsDeleted**: 軟刪除標記 (bit, NOT NULL, DEFAULT 0)
- **DeletedAt**: 刪除時間 (datetime2(7), NULL)
- **DeletedBy**: 刪除者 ID (int, NULL)
- **DeleteReason**: 刪除原因 (nvarchar(500), NULL)

#### 3.2.3 核銷記錄系統 (EVoucherRedeemLog)

`json
{
  "RedeemID": 1,
  "EVoucherID": 1,
  "TokenID": 1,
  "UserID": 10000042,
  "ScannedAt": "2025-05-30 23:53:14",
  "Status": "APPROVED",
  "IsDeleted": 0,
  "DeletedAt": null,
  "DeletedBy": null,
  "DeleteReason": null
}
`

**欄位說明**:

- **RedeemID**: 核銷記錄唯一識別碼 (int, IDENTITY, PK)
- **EVoucherID**: 被核銷的電子禮券 ID (int, NOT NULL, FK → EVoucher.EVoucherID, CASCADE DELETE)
- **TokenID**: 使用的核銷憑證 ID (int, NULL, FK → EVoucherToken.TokenID)
- **UserID**: 核銷操作的使用者 ID (int, NOT NULL, FK → Users.User_ID)
- **ScannedAt**: 核銷掃描時間 (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- **Status**: 核銷狀態 (nvarchar(20), NOT NULL)
  - 可選值: 'APPROVED', 'REJECTED', 'EXPIRED', 'ALREADYUSED', 'REVOKED' (大寫)
- **IsDeleted**: 軟刪除標記 (bit, NOT NULL, DEFAULT 0)
- **DeletedAt**: 刪除時間 (datetime2(7), NULL)
- **DeletedBy**: 刪除者 ID (int, NULL)
- **DeleteReason**: 刪除原因 (nvarchar(500), NULL)

**CHECK Constraint**:
- CK_EVoucherRedeemLog_Status: Status 必須是 'REVOKED', 'REJECTED', 'EXPIRED', 'ALREADYUSED', 'APPROVED' 其中之一（大寫）

### 3.3 商城優惠券系統詳細分析

#### 3.3.1 優惠券序號格式

根據 這裡有 MinGame Area 和管理者權限相關資料庫結構及種子資料.sql 分析，商城優惠券序號格式為：
`CPN-{年月}-{6位隨機碼}`

**範例序號**:

- CPN-2508-AAG049 - 2025 年 8 月發行的優惠券
- CPN-2309-EPV363 - 2023 年 9 月發行的優惠券
- CPN-2412-QBW875 - 2024 年 12 月發行的優惠券

#### 3.3.2 優惠券狀態管理

`json
{
  "CouponID": 599,
  "CouponCode": "CPN-2508-AAG049",
  "CouponTypeID": 5,
  "UserID": 10000171,
  "IsUsed": 0,
  "AcquiredTime": "2025-08-09 05:14:29",
  "UsedTime": null,
  "UsedInOrderID": null,
  "IsDeleted": 0,
  "DeletedAt": null,
  "DeletedBy": null,
  "DeleteReason": null
}
`

**欄位說明**:

- **CouponID**: 優惠券唯一識別碼 (int, IDENTITY, PK)
- **CouponCode**: 優惠券序號 (nvarchar(50), NOT NULL, UNIQUE)
- **CouponTypeID**: 優惠券類型 ID (int, NOT NULL, FK → CouponType.CouponTypeID)
- **UserID**: 持有者會員 ID (int, NOT NULL, FK → Users.User_ID)
- **IsUsed**: 使用狀態 (bit, NOT NULL, DEFAULT 0) - 0=未使用, 1=已使用
- **AcquiredTime**: 獲得時間 (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- **UsedTime**: 使用時間 (datetime2(7), NULL, DEFAULT sysutcdatetime()) - 未使用時為 null
- **UsedInOrderID**: 使用的訂單 ID (int, NULL) - 未使用時為 null
- **IsDeleted**: 軟刪除標記 (bit, NOT NULL, DEFAULT 0)
- **DeletedAt**: 刪除時間 (datetime2(7), NULL)
- **DeletedBy**: 刪除者 ID (int, NULL)
- **DeleteReason**: 刪除原因 (nvarchar(500), NULL)

**CHECK Constraints**:
- CK_Coupon_IsUsed: IsUsed 必須是 0 或 1
- CK_Coupon_UsedFields: IsUsed=0 時 UsedTime 和 UsedInOrderID 必須為 NULL；IsUsed=1 時兩者必須有值

**索引**:
- UQ_Coupon_CouponCode (UNIQUE) on CouponCode
- IX_Coupon_IsDeleted on IsDeleted
- IX_Coupon_user_used on (UserID, IsUsed, AcquiredTime)

### 3.4 錢包異動記錄系統 (WalletHistory)

#### 3.4.1 異動記錄格式

`json
{
  "LogID": 1,
  "UserID": 10000001,
  "ChangeType": "Point",
  "PointsChanged": -25,
  "ItemCode": null,
  "Description": "遊戲獎勵點數",
  "ChangeTime": "2024-04-03 11:03:00",
  "IsDeleted": 0,
  "DeletedAt": null,
  "DeletedBy": null,
  "DeleteReason": null
}
`

**欄位說明**:

- **LogID**: 記錄唯一識別碼 (int, IDENTITY, PK)
- **UserID**: 會員 ID (int, NOT NULL, FK → Users.User_ID)
- **ChangeType**: 異動類型 (nvarchar(20), NOT NULL) - Point/Coupon/EVoucher
- **PointsChanged**: 點數變動量 (int, NOT NULL) - 正數=增加, 負數=減少
- **ItemCode**: 相關物品代碼 (nvarchar(50), NULL) - 優惠券或禮券序號
- **Description**: 異動描述 (nvarchar(255), NULL)
- **ChangeTime**: 異動時間 (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- **IsDeleted**: 軟刪除標記 (bit, NOT NULL, DEFAULT 0)
- **DeletedAt**: 刪除時間 (datetime2(7), NULL)
- **DeletedBy**: 刪除者 ID (int, NULL)
- **DeleteReason**: 刪除原因 (nvarchar(500), NULL)

**索引**:
- IX_WalletHistory_IsDeleted on IsDeleted
- IX_WalletHistory_type_time on (ChangeType, ChangeTime)
- IX_WalletHistory_user_time on (UserID, ChangeTime)

#### 3.4.2 異動類型分析

根據 這裡有 MinGame Area 和管理者權限相關資料庫結構及種子資料.sql 分析，異動類型包括：

**點數異動 (Point)**:

- 遊戲獎勵點數
- 簽到獲得點數
- 寵物升級獎勵點數
- 兌換優惠券/禮券扣除點數

**優惠券異動 (Coupon)**:

- 獲得商城優惠券
- 使用商城優惠券

**電子禮券異動 (EVoucher)**:

- 獲得電子禮券
- 使用電子禮券

## 4. 前台功能詳細描述

### 4.1 會員點數系統 (User_Wallet)

#### 4.1.1 功能清單

**Client 端功能**:

- 查看當前會員點數餘額
- 使用會員點數兌換商城優惠券及電子優惠券
- 查看目前擁有商城優惠券
- 查看目前擁有電子優惠券
- 使用電子優惠券（qrcode 或 barcode 形式顯示，給店員核銷）
- 查看收支明細（含會員點數之得到、花費；商城優惠券之得到、使用；電子優惠券之得到、使用之時間及點數/張數/種類等等）

#### 4.1.2 商業規則

- **點數規則**: 會員點數不可為負數，調整需記錄原因
- **兌換規則**: 兌換優惠券/禮券需檢查點數餘額是否足夠
- **券類管理**: 優惠券僅限官方商城使用，禮券可於實體店核銷
- **安全機制**: 所有點數變動需在資料庫交易中執行，防止併發問題

#### 4.1.3 技術實作

- **Controller**: WalletController 處理所有錢包相關請求
- **Service**: WalletService 處理業務邏輯
- **Repository**: WalletRepository 處理資料存取
- **View**: 使用 Razor 語法，支援響應式設計

### 4.2 每日簽到系統 (UserSignInStats)

#### 4.2.1 功能清單

**Client 端功能**:

- 查看月曆型簽到簿並執行簽到
- 查看簽到歷史紀錄（含何時簽到及簽到得到的獎品，有可能是會員點數、寵物經驗值、商城優惠券）

#### 4.2.2 商業規則

- **簽到限制**: 每日限簽到一次，以 Asia/Taipei 時區為準
- **獎勵機制**:
  - 平日：+20 點數，+0 經驗
  - 假日：+30 點數，+200 經驗
  - 連續 7 天：額外 +40 點數，+300 經驗
  - 當月全勤：額外 +200 點數，+2000 經驗，+1 張商城優惠券

#### 4.2.3 技術實作

- **Controller**: CheckInController 處理簽到相關請求
- **Service**: CheckInService 處理簽到業務邏輯
- **SignalR**: CheckInHub 負責即時通知
- **View**: 使用 JavaScript 繪製互動式月曆

### 4.3 寵物養成系統 (Pet)

#### 4.3.1 功能清單

**Client 端功能**:

- 寵物名字修改
- 寵物互動（餵食/洗澡/玩耍/哄睡）
- 寵物換膚色（扣會員點數）
- 寵物換背景（可能有免費背景也可能有須扣會員點數的背景）

#### 4.3.2 商業規則

- **五維屬性**: 飢餓、心情、體力、清潔、健康，範圍 0-100
- **升級公式**:
  - Level 1–10：EXP = 40 × level + 60
  - Level 11–100：EXP = 0.8 × level² + 380
  - Level ≥ 101：EXP = 285.69 × (1.06^level)
- **換膚色費用**: 每次換色需消耗 2000 會員點數
- **升級獎勵**: 寵物等級 1–10 每升級 +10 點會員點數、11–20 每升級 +20 點，以此類推
- **每日衰減**: 每日凌晨 00:00，飢餓值 -20、心情值 -30、體力值 -10、清潔值 -20

#### 4.3.3 Pet 資料表結構

**主鍵**: PetID (int, IDENTITY)
**外鍵**: UserID → Users.User_ID (NO_ACTION)

**欄位**:
- PetID (int, NOT NULL, IDENTITY) - PK
- UserID (int, NOT NULL, FK)
- PetName (nvarchar(50), NOT NULL)
- Level (int, NOT NULL)
- LevelUpTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- Experience (int, NOT NULL)
- Hunger (int, NOT NULL) - 範圍 0-100
- Mood (int, NOT NULL) - 範圍 0-100
- Stamina (int, NOT NULL) - 範圍 0-100
- Cleanliness (int, NOT NULL) - 範圍 0-100
- Health (int, NOT NULL) - 範圍 0-100
- SkinColor (varchar(7), NOT NULL) - 格式: #RRGGBB
- SkinColorChangedTime (datetime2(7), NOT NULL)
- BackgroundColor (nvarchar(20), NOT NULL)
- BackgroundColorChangedTime (datetime2(7), NOT NULL)
- PointsChanged_SkinColor (int, NOT NULL)
- PointsChanged_BackgroundColor (int, NOT NULL)
- PointsGained_LevelUp (int, NOT NULL)
- PointsGainedTime_LevelUp (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- CurrentExperience (int, NOT NULL, DEFAULT 0)
- ExperienceToNextLevel (int, NULL)
- TotalPointsGained_LevelUp (int, NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**CHECK Constraints**:
- CK_Pet_Hunger: Hunger >= 0 AND Hunger <= 100
- CK_Pet_Mood: Mood >= 0 AND Mood <= 100
- CK_Pet_Stamina: Stamina >= 0 AND Stamina <= 100
- CK_Pet_Cleanliness: Cleanliness >= 0 AND Cleanliness <= 100
- CK_Pet_Health: Health >= 0 AND Health <= 100

#### 4.3.4 寵物相關設定表

##### PetSkinColorCostSettings - 寵物膚色價格設定
**欄位**:
- SettingId (int, IDENTITY, PK)
- ColorCode (varchar(7), NOT NULL) - 格式: #RRGGBB
- ColorName (nvarchar(50), NOT NULL)
- PointsCost (int, NOT NULL, DEFAULT 2000) - >= 0
- Rarity (nvarchar(20), NOT NULL, DEFAULT '普通') - 可選值: '普通', '稀有', '史詩', '傳說', '限定'
- Description (nvarchar(500), NULL)
- PreviewImagePath (nvarchar(500), NULL)
- ColorHex (varchar(7), NULL)
- IsActive (bit, NOT NULL, DEFAULT 1)
- DisplayOrder (int, NOT NULL, DEFAULT 0)
- IsFree (bit, NOT NULL, DEFAULT 0)
- IsLimitedEdition (bit, NOT NULL, DEFAULT 0)
- AvailableFrom (datetime2(7), NULL)
- AvailableUntil (datetime2(7), NULL)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- UpdatedBy (int, NULL)

**CHECK Constraints**:
- CK_PetSkinColorCostSettings_PointsCost: PointsCost >= 0
- CK_PetSkinColorCostSettings_ColorCode: ColorCode LIKE '#%' AND LEN(ColorCode) >= 4
- CK_PetSkinColorCostSettings_Rarity: Rarity IN ('普通', '稀有', '史詩', '傳說', '限定')

##### PetBackgroundCostSettings - 寵物背景價格設定
**欄位**:
- SettingId (int, IDENTITY, PK)
- BackgroundCode (nvarchar(50), NOT NULL)
- BackgroundName (nvarchar(100), NOT NULL)
- PointsCost (int, NOT NULL) - >= 0
- Description (nvarchar(500), NULL)
- PreviewImagePath (nvarchar(200), NULL)
- IsActive (bit, NOT NULL, DEFAULT 1)
- DisplayOrder (int, NULL, DEFAULT 0)
- Rarity (nvarchar(20), NULL)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- UpdatedBy (int, NULL)

**CHECK Constraints**:
- CK__PetBackgr__Point__541767F8: PointsCost >= 0

##### PetLevelRewardSettings - 寵物升級獎勵規則
**欄位**:
- SettingId (int, IDENTITY, PK)
- LevelRangeStart (int, NOT NULL) - > 0
- LevelRangeEnd (int, NOT NULL) - >= LevelRangeStart
- PointsReward (int, NOT NULL) - 範圍 0-999999
- Description (nvarchar(500), NULL)
- IsActive (bit, NOT NULL, DEFAULT 1)
- DisplayOrder (int, NOT NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- UpdatedBy (int, NULL)

**CHECK Constraints**:
- CK_PetLevelRewardSettings_LevelRange: LevelRangeStart > 0 AND LevelRangeEnd >= LevelRangeStart
- CK_PetLevelRewardSettings_PointsReward: PointsReward >= 0 AND PointsReward <= 999999

#### 4.3.5 技術實作

- **Controller**: PetController 處理寵物相關請求
- **Service**: PetInteractionService 處理寵物互動邏輯
- **WebSocket**: 實現寵物狀態即時更新
- **View**: 使用 Vue.js 組件實現互動式寵物介面

### 4.4 小遊戲系統 (MiniGame)

#### 4.4.1 功能清單

**Client 端功能**:

- 出發冒險
- 查看遊戲紀錄（時間（startTime、endTime）/輸/贏/獲得獎勵（可能是會員點數、寵物經驗值、商城優惠券））

#### 4.4.2 商業規則

- **遊戲次數限制**: 每日最多 3 次遊戲，以 Asia/Taipei 時區每日 00:00 重置
- **關卡設計**:
  - 第 1 關：怪物數量 6、移動速度 1 倍；獎勵 +100 寵物經驗值，+10 會員點數
  - 第 2 關：怪物數量 8、移動速度 1.5 倍；獎勵 +200 寵物經驗值，+20 會員點數
  - 第 3 關：怪物數量 10、移動速度 2 倍；獎勵 +300 寵物經驗值，+30 會員點數，+1 張商城優惠券
- **健康狀態檢查**: 冒險開始前檢查寵物狀態，若飢餓、心情、體力、清潔、健康任一屬性值為 0，則無法開始冒險

#### 4.4.3 MiniGame 資料表結構

**主鍵**: PlayID (int, IDENTITY)
**外鍵**:
- PetID → Pet.PetID (NO_ACTION)
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- PlayID (int, NOT NULL, IDENTITY) - PK
- UserID (int, NOT NULL, FK)
- PetID (int, NOT NULL, FK)
- Level (int, NOT NULL) - 關卡等級
- MonsterCount (int, NOT NULL) - 怪物數量
- SpeedMultiplier (decimal(5,2), NOT NULL) - 速度倍率
- Result (nvarchar(20), NOT NULL) - 遊戲結果 (Win/Lose/Abort)
- ExpGained (int, NOT NULL) - 獲得經驗值
- ExpGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- PointsGained (int, NOT NULL) - 獲得點數
- PointsGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- CouponGained (nvarchar(50), NOT NULL) - 獲得優惠券
- CouponGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- HungerDelta (int, NOT NULL) - 飢餓值變化
- MoodDelta (int, NOT NULL) - 心情值變化
- StaminaDelta (int, NOT NULL) - 體力值變化
- CleanlinessDelta (int, NOT NULL) - 清潔值變化
- StartTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- EndTime (datetime2(7), NULL, DEFAULT sysutcdatetime())
- Aborted (bit, NOT NULL, DEFAULT 0) - 是否中途退出
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

#### 4.4.4 技術實作

- **Controller**: GameController 處理遊戲相關請求
- **Service**: GameEngineService 處理遊戲核心邏輯
- **Canvas**: 使用 HTML5 Canvas 實現遊戲畫面
- **SignalR**: 實現遊戲狀態的即時同步

## 5. 後台管理功能詳細描述

### 5.1 會員點數系統後台管理

#### 5.1.1 查詢功能

- **查詢會員點數** - 查看特定會員的當前點數餘額
- **查詢會員擁有商城優惠券** - 查看會員持有的所有商城優惠券
- **查詢會員擁有電子禮券** - 查看會員持有的所有電子禮券

#### 5.1.2 管理功能

- **調整會員點數** - 手動增加或減少會員點數
- **調整會員擁有商城優惠券** - 發放或刪除會員的商城優惠券
- **調整會員擁有電子禮券** - 發放或刪除會員的電子禮券
- **查看會員收支明細** - 完整的財務記錄，包含：
  - 會員點數的得到、花費記錄
  - 商城優惠券的得到、使用記錄
  - 電子禮券的得到、使用記錄
  - 時間、點數/張數/種類等詳細資訊

### 5.2 會員簽到系統後台管理

#### 5.2.1 規則設定

- **簽到規則設定** - 配置簽到獎勵規則和條件

#### 5.2.2 記錄管理

- **查看會員簽到紀錄** - 查看所有會員的簽到歷史
- **手動調整會員簽到紀錄** - 管理員可以：
  - 刪除特定會員的簽到紀錄
  - 新增特定會員的簽到紀錄

### 5.3 寵物系統後台管理

#### 5.3.1 系統規則設定

- **整體寵物系統規則設定** - 配置：
  - 升級規則（經驗值需求、等級上限等）
  - 互動狀態增益規則（餵食、洗澡、玩耍、哄睡的效果）
  - 寵物外型選項（可選擇的寵物類型）
  - 寵物顏色選項（可選擇的膚色）
  - 寵物換色所需點數（不同顏色的點數消耗）
  - 寵物背景選項（可選擇的背景）
  - 寵物換背景所需點數（不同背景的點數消耗）

#### 5.3.2 個別會員管理

- **會員個別寵物設定** - 手動調整特定會員的寵物：
  - 寵物名稱
  - 膚色
  - 背景
  - 經驗值
  - 等級
  - 五大狀態值數值（飢餓、心情、體力、清潔度、健康）

#### 5.3.3 查詢功能

- **會員個別寵物基本資料清單含查詢** - 查看：
  - 寵物名稱
  - 目前膚色
  - 目前背景
  - 目前經驗值
  - 目前等級
  - 目前五大狀態值數值
- **換膚色紀錄查詢** - 查看寵物膚色變更歷史
- **換背景色紀錄查詢** - 查看寵物背景變更歷史

### 5.4 小遊戲系統後台管理

#### 5.4.1 遊戲規則設定

- **遊戲規則設定** - 配置：
  - 每關怪物數量
  - 每關怪物行進速率
  - 獎勵設定（可能是會員點數、寵物經驗值、商城優惠券）
  - 每日遊戲次數限制（預設一天三次）

#### 5.4.2 遊戲記錄管理

- **查看會員遊戲紀錄** - 查看所有會員的遊戲記錄，包含：
  - 時間（startTime、endTime）
  - 遊戲結果（輸/lose、贏/win、中退/abort）
  - 獲得獎勵（可能是會員點數、寵物經驗值、商城優惠券）

## 6. 權限控制架構

### 6.1 權限角色定義

根據 這裡有 MinGame Area 和管理者權限相關資料庫結構及種子資料.sql 中的 ManagerRolePermission 表定義：

| 角色 ID | 角色名稱             | 管理員權限 | 使用者管理 | 購物管理 | 訊息管理 | 寵物管理 | 客服   |
| ------- | -------------------- | ---------- | ---------- | -------- | -------- | -------- | ------ |
| 1       | 管理者平台管理人員   | ✅ (1)     | ✅ (1)     | ✅ (1)   | ✅ (1)   | ✅ (1)   | ✅ (1) |
| 2       | 使用者與論壇管理精理 | ❌ (0)     | ✅ (1)     | ❌ (0)   | ✅ (1)   | ❌ (0)   | ✅ (1) |
| 3       | 商城與寵物管理經理   | ❌ (0)     | ❌ (0)     | ✅ (1)   | ❌ (0)   | ✅ (1)   | ❌ (0) |
| 4       | 使用者平台管理人員   | ❌ (0)     | ✅ (1)     | ❌ (0)   | ❌ (0)   | ❌ (0)   | ❌ (0) |
| 5       | 購物平台管理人員     | ❌ (0)     | ❌ (0)     | ✅ (1)   | ❌ (0)   | ❌ (0)   | ❌ (0) |
| 6       | 論壇平台管理人員     | ❌ (0)     | ❌ (0)     | ❌ (0)   | ✅ (1)   | ❌ (0)   | ❌ (0) |
| 7       | 寵物平台管理人員     | ❌ (0)     | ❌ (0)     | ❌ (0)   | ❌ (0)   | ✅ (1)   | ❌ (0) |
| 8       | 客服與交友管理員     | ❌ (0)     | ❌ (0)     | ❌ (0)   | ❌ (0)   | ❌ (0)   | ✅ (1) |

### 6.2 權限對應關係

- **User_Wallet 模組**: 需要 UserStatusManagement 權限
- **UserSignInStats 模組**: 需要 UserStatusManagement 權限
- **Pet 模組**: 需要 Pet_Rights_Management 權限
- **MiniGame 模組**: 需要 UserStatusManagement 權限

### 6.3 管理員帳號資料

根據 這裡有 MinGame Area 和管理者權限相關資料庫結構及種子資料.sql 中的測試資料：

#### 測試管理員帳號

- **張志明** (zhang_zhiming_01 / AdminPass001@) - 角色 ID: 1 (管理者平台管理人員)
- **李小華** (li_xiaohua_02 / SecurePass002#) - 角色 ID: 2 (使用者與論壇管理精理)
- **王美玲** (wang_meiling_03 / StrongPwd003!) - 角色 ID: 3 (商城與寵物管理經理)
- **陳大偉** (chen_dawei_04 / SafeLogin004$) - 角色 ID: 4 (使用者平台管理人員)
- **林雅婷** (lin_yating_05 / Manager005%) - 角色 ID: 5 (購物平台管理人員)
- **John Anderson** (john_anderson_06 / AdminJohn006^) - 角色 ID: 6 (論壇平台管理人員)
- **劉建國** (liu_jianguo_07 / BuildNation007&) - 角色 ID: 7 (寵物平台管理人員)
- **Sarah Johnson** (sarah_johnson_08 / SarahMgr008\*) - 角色 ID: 8 (客服與交友管理員)

#### 測試會員帳號

- **DragonKnight88** (dragonknight88 / Password001@) - 一般會員

### 6.4 權限控制實作細節

#### 6.4.1 資料庫層級權限控制

`sql
-- 檢查管理員權限的 SQL 查詢範例
SELECT 
    md.Manager_Id,
    md.Manager_Name,
    md.Manager_Account,
    mrp.role_name,
    mrp.UserStatusManagement,
    mrp.Pet_Rights_Management,
    mrp.ShoppingPermissionManagement
FROM ManagerData md
JOIN ManagerRole mr ON md.Manager_Id = mr.Manager_Id
JOIN ManagerRolePermission mrp ON mr.ManagerRole_Id = mrp.ManagerRole_Id
WHERE md.Manager_Account = @ManagerAccount
`

#### 6.4.2 應用程式層級權限驗證

`csharp
[Area("MiniGame")]
[Authorize]
public class MiniGameAdminController : Controller
{
[RequirePermission("UserStatusManagement")]
public IActionResult ManageUserWallet() { }

    [RequirePermission("Pet_Rights_Management")]
    public IActionResult ManagePet() { }

    [RequirePermission("ShoppingPermissionManagement")]
    public IActionResult ManageCoupons() { }

}
`

## 7. 業務邏輯與公式

### 7.1 簽到獎勵公式

`平日簽到: +20 點數, +0 經驗
假日簽到: +30 點數, +200 經驗
連續 7 天: 額外 +40 點數, +300 經驗
當月全勤: 額外 +200 點數, +2000 經驗, +1 張商城優惠券`

### 7.2 寵物升級公式

`Level 1–10: EXP = 40 × level + 60
Level 11–100: EXP = 0.8 × level² + 380
Level ≥ 101: EXP = 285.69 × (1.06^level)`

### 7.3 寵物升級點數獎勵

`Level 1-10: 每升級 +10 點會員點數
Level 11-20: 每升級 +20 點會員點數
Level 21-30: 每升級 +30 點會員點數
...以此類推
Level 241-250: 每升級 +250 點會員點數（上限）`

### 7.4 每日狀態全滿獎勵

`寵物若於每日首次同時達到飢餓、心情、體力、清潔值皆 100，
則額外獲得 100 點寵物經驗值`

### 7.5 寵物互動效果

`餵食: 飢餓值 +10
洗澡: 清潔值 +10
哄睡: 心情值 +10
休息: 體力值 +10`

### 7.6 每日屬性衰減

`
每日凌晨 00:00:

- 飢餓值 -20
- 心情值 -30
- 體力值 -10
- 清潔值 -20
  `

## 8. 技術實作規範

### 8.1 開發規範

- **語言規則**: 人機介面使用繁體中文 (zh-TW)，程式識別符、檔名、路徑、SQL/CLI 關鍵字不可翻譯
- **架構模式**: 三層式架構設計（Presentation Layer、Business Logic Layer、Data Access Layer）
- **編碼規範**: UTF-8 (prefer no BOM)
- **檔案限制**: 每批 ≤3 檔或 ≤400 行

### 8.2 資料庫規範

- **權威來源**: 實際 SQL Server 資料庫為唯一真實來源，參考 db_schema_summary.md (2025-10-27 查詢)
- **資料庫伺服器**: DESKTOP-8HQIS1S\SQLEXPRESS
- **資料庫名稱**: GameSpacedatabase
- **SQL Server 版本**: Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 Express Edition
- **禁止修改**: 不得使用 EF Migrations 或修改 schema
- **讀取方式**: 使用 AsNoTracking() 進行讀取操作
- **資料型別規範**:
  - 字串: nvarchar (支援 Unicode), varchar (純 ASCII)
  - 數值: int, decimal(18,2) (金額), decimal(5,2) (倍率)
  - 布林: bit
  - 日期時間: datetime2(7) (主要), datetime2(6) (Users.Create_Account)
- **軟刪除**: 所有 MiniGame Area 表格都實作軟刪除 (IsDeleted, DeletedAt, DeletedBy, DeleteReason)
- **審計追蹤**: 部分表格有 CreatedAt, UpdatedAt, UpdatedBy
- **時間戳記**: 多數時間欄位使用 DEFAULT sysutcdatetime() 自動填入

### 8.3 權限控制實作

`csharp
[Area("MiniGame")]
[Authorize]
public class MiniGameAdminController : Controller
{
[RequirePermission("UserStatusManagement")]
public IActionResult ManageUserWallet() { }

    [RequirePermission("Pet_Rights_Management")]
    public IActionResult ManagePet() { }

    [RequirePermission("ShoppingPermissionManagement")]
    public IActionResult ManageCoupons() { }

}
`

### 8.4 服務層架構

`csharp
public interface IMiniGameAdminService
{
// 會員點數服務
Task<UserWalletViewModel> GetUserWallet(int userId);
Task UpdateUserPoints(int userId, int points);

    // 簽到系統服務
    Task<SignInRulesViewModel> GetSignInRules();
    Task UpdateSignInRules(SignInRulesViewModel rules);

    // 寵物系統服務
    Task<PetRulesViewModel> GetPetRules();
    Task UpdatePetRules(PetRulesViewModel rules);

    // 小遊戲服務
    Task<GameRulesViewModel> GetGameRules();
    Task UpdateGameRules(GameRulesViewModel rules);

}
`

## 9. 安全控制機制

### 9.1 前端權限控制

`html
@if (HasPermission("UserStatusManagement"))
{
<a href="@Url.Action("ManageUserWallet")" class="btn btn-primary">錢包管理</a>
}

@if (HasPermission("Pet_Rights_Management"))
{
<a href="@Url.Action("ManagePet")" class="btn btn-primary">寵物管理</a>
}
`

### 9.2 後端權限驗證

`csharp
[HttpPost]
[RequirePermission("UserStatusManagement")]
public async Task<IActionResult> UpdateUserPoints(int userId, int points)
{
    // 只有具備 UserStatusManagement 權限的管理員才能執行
    // 實際更新邏輯
}
`

### 9.3 權限管理最佳實踐

- **最小權限原則**: 每個管理員只獲得完成工作所需的最小權限
- **權限分離**: 不同功能模組使用不同的權限控制
- **審計追蹤**: 記錄所有權限相關操作，追蹤權限變更歷史
- **帳號安全**: 支援帳號鎖定、登入失敗計數、密碼複雜度驗證

### 9.4 資料庫安全機制

- **唯一性約束**: Manager_Account 和 Manager_Email 必須唯一
- **外鍵約束**: 確保 ManagerRole 表與 ManagerData 和 ManagerRolePermission 的關聯完整性
- **預設值**: 設定合理的預設值，如 Manager_EmailConfirmed = 0, Manager_LockoutEnabled = 1

## 10. 部署與維護

### 10.1 部署要求

- **開發環境**: Visual Studio 2022+ 和 SQL Server 2022 Express Edition
- **資料庫伺服器**: DESKTOP-8HQIS1S\SQLEXPRESS
- **資料庫名稱**: GameSpacedatabase
- **資料庫初始化**:
  - 資料庫結構已建立完成，共 20 張表格
  - MiniGame Area 主要表格：16 張
  - 使用者/權限相關表格：4 張
- **連線設定**: 設定 appsettings.json DefaultConnection
- **注意事項**:
  - 嚴禁使用 EF Migrations 修改資料庫結構
  - 所有結構變更必須直接在 SQL Server 上執行

### 10.2 測試帳號

- **管理員帳號**:
  - zhang_zhiming_01 / AdminPass001@ (全權限)
  - li_xiaohua_02 / SecurePass002# (使用者與論壇管理)
  - wang_meiling_03 / StrongPwd003! (商城與寵物管理)
- **一般會員**: dragonknight88 / Password001@

### 10.3 監控與維護

- **健康檢查**: /healthz/db 回傳 {status:'ok'} 或錯誤
- **日誌記錄**: 使用 Serilog 記錄系統操作
- **效能監控**: 監控錢包、簽到、寵物、遊戲等熱點功能
- **權限審計**: 定期檢查管理員權限分配是否合理

## 11. 總結

MiniGame Area 是 GameSpace 平台的核心功能區域，提供完整的會員互動體驗。透過嚴格的權限控制、清晰的業務邏輯和穩定的技術架構，確保系統能夠安全、高效地為用戶提供優質的服務。

### 11.1 文件更新說明

本文件於 2025-10-27 根據實際資料庫結構進行全面更新，確保所有技術細節與實際環境一致：

**更新內容**:
- 所有資料表欄位定義更新為實際資料庫結構
- 補充完整的約束條件（CHECK Constraints）
- 補充完整的索引資訊
- 補充完整的外鍵關係
- 更新所有資料型別定義
- 更新預設值設定
- 補充軟刪除欄位 (IsDeleted, DeletedAt, DeletedBy, DeleteReason)
- 補充審計追蹤欄位 (CreatedAt, UpdatedAt, UpdatedBy)

**資料來源**:
- 實際資料庫：DESKTOP-8HQIS1S\SQLEXPRESS\GameSpacedatabase
- 參考文件：db_schema_summary.md (2025-10-27 查詢結果)
- 總表格數：20 張 (MiniGame Area 16 張 + 使用者/權限 4 張)

**保持不變**:
- 業務邏輯與功能描述
- 前後台功能規劃
- 權限控制架構
- 開發流程與規範

開發團隊應以本文檔為準進行開發和維護工作，確保實作與文件描述完全一致。

---

