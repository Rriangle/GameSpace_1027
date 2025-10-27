# MiniGame Area 資料庫完整結構文件

**生成日期：** 2025-10-21
**資料庫名稱：** GameSpacedatabase
**資料庫伺服器：** DESKTOP-8HQIS1S\SQLEXPRESS
**SQL Server 版本：** Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 (X64) Express Edition
**文件編碼：** UTF-8 with BOM

---

## 文件摘要統計

| 統計項目 | 數量 |
|---------|------|
| 總表數 | 18 |
| 總欄位數 | 241 |
| 總 Primary Keys | 19 (含 1 組 Composite Key) |
| 總 Foreign Keys | 22 |
| 功能模組數 | 6 |

---

## 資料表功能分組

### 1. Wallet System (錢包系統) - 2 tables
- `User_Wallet` - 使用者錢包主表
- `WalletHistory` - 錢包交易歷史記錄

### 2. Coupon System (優惠券系統) - 2 tables
- `CouponType` - 優惠券類型定義
- `Coupon` - 優惠券實例

### 3. E-Voucher System (電子禮券系統) - 4 tables
- `EVoucherType` - 電子禮券類型定義
- `EVoucher` - 電子禮券實例
- `EVoucherToken` - 禮券兌換令牌
- `EVoucherRedeemLog` - 禮券兌換記錄

### 4. Sign-In System (簽到系統) - 2 tables
- `SignInRule` - 簽到規則設定
- `UserSignInStats` - 使用者簽到記錄

### 5. Pet System (寵物系統) - 4 tables
- `Pet` - 寵物主表
- `PetSkinColorCostSettings` - 寵物膚色設定
- `PetBackgroundCostSettings` - 寵物背景設定
- `PetLevelRewardSettings` - 寵物升級獎勵設定

### 6. Mini-Game System (小遊戲系統) - 1 table
- `MiniGame` - 遊戲遊玩記錄

### 7. Manager/Admin System (管理員系統) - 3 tables
- `ManagerData` - 管理員帳號資料
- `ManagerRole` - 管理員角色關聯
- `ManagerRolePermission` - 角色權限定義

---

## Entity Relationship Diagram (ERD) 摘要

```
┌─────────────────────────────────────────────────────────────────┐
│                        Wallet System                            │
├─────────────────────────────────────────────────────────────────┤
│ Users ──< User_Wallet                                           │
│ Users ──< WalletHistory                                         │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                       Coupon System                             │
├─────────────────────────────────────────────────────────────────┤
│ CouponType ──< Coupon >── Users                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                     E-Voucher System                            │
├─────────────────────────────────────────────────────────────────┤
│ EVoucherType ──< EVoucher >── Users                             │
│ EVoucher ──< EVoucherToken                                      │
│ EVoucher ──< EVoucherRedeemLog >── EVoucherToken                │
│ EVoucherRedeemLog >── Users                                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      Sign-In System                             │
├─────────────────────────────────────────────────────────────────┤
│ CouponType <── SignInRule                                       │
│ Users ──< UserSignInStats                                       │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                        Pet System                               │
├─────────────────────────────────────────────────────────────────┤
│ Users ──< Pet ──< MiniGame >── Users                            │
│ PetSkinColorCostSettings (standalone configuration)             │
│ PetLevelRewardSettings (standalone configuration)               │
│ PetBackgroundCostSettings >── ManagerData (audit)               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    Manager/Admin System                         │
├─────────────────────────────────────────────────────────────────┤
│ ManagerData ──< ManagerRole >── ManagerRolePermission           │
└─────────────────────────────────────────────────────────────────┘
```

---

# 詳細資料表結構

## 一、Wallet System (錢包系統)

### 1.1 User_Wallet

**表格用途：** 儲存使用者錢包餘額資訊（每位使用者一筆記錄）

**Primary Key：** `User_Id`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| User_Id | Users | User_ID | FK_User_Wallet_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| User_Id | int | - | YES | - | 使用者 ID（主鍵） |
| User_Point | int | - | YES | ((0)) | 使用者點數餘額 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨用戶註冊自動建立）

---

### 1.2 WalletHistory

**表格用途：** 記錄所有錢包點數變動的交易歷史（增減點數、購買項目等）

⚠️ **重要澄清** (2025-10-21 Services 層修復發現):
- **WalletHistory 表沒有 `BalanceBefore`/`BalanceAfter` 欄位**
- 餘額變化記錄在 `Description` 欄位中，格式範例: `"每日簽到獎勵 (餘額：1000 → 1020)"`
- 所有 Services 層程式碼已修正為使用 Description 欄位記錄餘額變化
- SERVICES_FIX_PLAN.md 中的 WalletHistory.BalanceBefore/BalanceAfter 要求與實際資料庫不符

**Primary Key：** `LogID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| UserID | Users | User_ID | FK_WalletHistory_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| LogID | int | - | YES | - | 記錄 ID（主鍵、自動遞增） |
| UserID | int | - | YES | - | 使用者 ID |
| ChangeType | nvarchar | 20 | YES | - | 變動類型（例如：購買、獎勵、簽到、升級） |
| PointsChanged | int | - | YES | - | 點數變動量（正數為增加、負數為減少） |
| ItemCode | nvarchar | 50 | NO | - | 項目代碼（例如：優惠券代碼、商品 ID） |
| Description | nvarchar | 255 | NO | - | 交易描述 |
| ChangeTime | datetime2 | - | YES | (sysutcdatetime()) | 變動時間（UTC） |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨交易產生）

---

## 二、Coupon System (優惠券系統)

### 2.1 CouponType

**表格用途：** 定義優惠券類型範本（折扣類型、金額、消費門檻、兌換點數等）

**Primary Key：** `CouponTypeID`

**Foreign Keys：** 無

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| CouponTypeID | int | - | YES | - | 優惠券類型 ID（主鍵、自動遞增） |
| Name | nvarchar | 50 | YES | - | 優惠券名稱 |
| DiscountType | nvarchar | 20 | YES | - | 折扣類型（Amount=固定金額 / Percent=百分比） |
| DiscountValue | decimal | (18,2) | YES | - | 折扣值（金額或百分比，例如：100.00 或 0.15） |
| MinSpend | decimal | (18,2) | YES | - | 最低消費門檻 |
| ValidFrom | datetime2 | - | YES | - | 有效期開始時間 |
| ValidTo | datetime2 | - | YES | - | 有效期結束時間 |
| PointsCost | int | - | YES | - | 兌換所需點數 |
| Description | nvarchar | 600 | NO | - | 優惠券說明 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料範例（前 20 筆）：**

| ID | Name | DiscountType | DiscountValue | MinSpend | PointsCost |
|----|------|--------------|---------------|----------|------------|
| 1 | 新會員$100券 | Amount | 100.00 | 2000.00 | 10000 |
| 2 | 商城85折 | Percent | 0.15 | 1500.00 | 22500 |
| 3 | 滿$500折$50 | Amount | 50.00 | 500.00 | 5000 |
| 4 | 滿$1000折$120 | Amount | 120.00 | 1000.00 | 12000 |
| 5 | 生日券 | Amount | 150.00 | 1000.00 | 15000 |
| 6 | 會員專屬9折 | Percent | 0.10 | 1500.00 | 15000 |
| 7 | 電子書平台券$30 | Amount | 50.00 | 0.00 | 5000 |
| 8 | 生日限定商品買二送一 | Percent | 0.10 | 0.00 | 0 |
| 9 | 黑色星期五9折 | Percent | 0.10 | 0.00 | 0 |
| 10 | 夏日專屬生日券 | Amount | 50.00 | 2000.00 | 5000 |
| 11 | 指定商品85折 | Percent | 0.15 | 1000.00 | 15000 |
| 12 | App專屬$80券 | Amount | 20.00 | 300.00 | 2000 |
| 13 | 點數回饋5% | Percent | 0.05 | 800.00 | 4000 |
| 14 | 滿$2000折$300 | Amount | 200.00 | 0.00 | 20000 |
| 15 | 學生專屬9折 | Percent | 0.10 | 1500.00 | 15000 |
| 16 | 滿$800折$120 | Amount | 150.00 | 2000.00 | 15000 |
| 17 | 商城第二件6折 | Percent | 0.40 | 1000.00 | 40000 |
| 18 | 滿$1500送$100券 | Amount | 120.00 | 500.00 | 12000 |
| 19 | 年中慶88折 | Percent | 0.12 | 1500.00 | 18000 |
| 20 | 滿$300折$30 | Amount | 120.00 | 1500.00 | 12000 |

---

### 2.2 Coupon

**表格用途：** 儲存發放給使用者的優惠券實例（每張券有唯一代碼、使用狀態等）

**Primary Key：** `CouponID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| CouponTypeID | CouponType | CouponTypeID | FK_Coupon_CouponType |
| UserID | Users | User_ID | FK_Coupon_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| CouponID | int | - | YES | - | 優惠券 ID（主鍵、自動遞增） |
| CouponCode | nvarchar | 50 | YES | - | 優惠券唯一代碼 |
| CouponTypeID | int | - | YES | - | 優惠券類型 ID（FK） |
| UserID | int | - | YES | - | 擁有者使用者 ID（FK） |
| IsUsed | bit | - | YES | - | 是否已使用（0=未使用 / 1=已使用） |
| AcquiredTime | datetime2 | - | YES | (sysutcdatetime()) | 取得時間 |
| UsedTime | datetime2 | - | NO | (sysutcdatetime()) | 使用時間 |
| UsedInOrderID | int | - | NO | - | 使用於哪張訂單（FK to Orders） |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨用戶兌換產生）

---

## 三、E-Voucher System (電子禮券系統)

### 3.1 EVoucherType

**表格用途：** 定義電子禮券類型範本（面額、兌換點數、庫存數量等）

**Primary Key：** `EVoucherTypeID`

**Foreign Keys：** 無

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| EVoucherTypeID | int | - | YES | - | 電子禮券類型 ID（主鍵、自動遞增） |
| Name | nvarchar | 50 | YES | - | 禮券名稱 |
| ValueAmount | decimal | (18,2) | YES | - | 禮券面額 |
| ValidFrom | datetime2 | - | YES | - | 有效期開始時間 |
| ValidTo | datetime2 | - | YES | - | 有效期結束時間 |
| PointsCost | int | - | YES | - | 兌換所需點數 |
| TotalAvailable | int | - | YES | - | 總庫存數量 |
| Description | nvarchar | 600 | NO | - | 禮券說明 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料範例（前 20 筆）：**

| ID | Name | ValueAmount | PointsCost |
|----|------|-------------|------------|
| 1 | 現金券$100 | 100.00 | 10000 |
| 2 | 現金券$200 | 200.00 | 20000 |
| 3 | 現金券$300 | 300.00 | 30000 |
| 4 | 現金券$500 | 500.00 | 50000 |
| 5 | 星巴克咖啡-特大杯(M) | 200.00 | 20000 |
| 6 | 星巴克咖啡-拿鐵(M) | 300.00 | 30000 |
| 7 | 路易莎咖啡-拿鐵 | 300.00 | 30000 |
| 8 | 路易莎咖啡-美式 | 250.00 | 25000 |
| 9 | 動漫祭門票$1000 | 1000.00 | 100000 |
| 10 | 威秀電影券$150 | 150.00 | 15000 |
| 11 | 誠品圖書券$500 | 500.00 | 50000 |
| 12 | 博客來書券$300 | 300.00 | 30000 |
| 13 | 年貨大街券$120 | 120.00 | 12000 |
| 14 | 夏日電影券$200 | 200.00 | 20000 |
| 15 | 春節餐廳券$400 | 400.00 | 40000 |
| 16 | 加油站禮券$200 | 200.00 | 20000 |
| 17 | 早包店咖啡-可頌 | 200.00 | 20000 |
| 18 | 連鎖咖啡-冰淇淋拿鐵 | 200.00 | 20000 |
| 19 | 甜點店咖啡-草莓 | 300.00 | 30000 |
| 20 | 運動中心券$300 | 300.00 | 30000 |

---

### 3.2 EVoucher

**表格用途：** 儲存發放給使用者的電子禮券實例

**Primary Key：** `EVoucherID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| EVoucherTypeID | EVoucherType | EVoucherTypeID | FK_EVoucher_EVoucherType |
| UserID | Users | User_ID | FK_EVoucher_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| EVoucherID | int | - | YES | - | 電子禮券 ID（主鍵、自動遞增） |
| EVoucherCode | nvarchar | 50 | YES | - | 禮券唯一代碼 |
| EVoucherTypeID | int | - | YES | - | 禮券類型 ID（FK） |
| UserID | int | - | YES | - | 擁有者使用者 ID（FK） |
| IsUsed | bit | - | YES | - | 是否已使用（0=未使用 / 1=已使用） |
| AcquiredTime | datetime2 | - | YES | (sysutcdatetime()) | 取得時間 |
| UsedTime | datetime2 | - | NO | (sysutcdatetime()) | 使用時間 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨用戶兌換產生）

---

### 3.3 EVoucherToken

**表格用途：** 儲存電子禮券的安全兌換令牌（用於 QR Code 掃描兌換）

**Primary Key：** `TokenID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| EVoucherID | EVoucher | EVoucherID | FK_EVoucherToken_EVoucher |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| TokenID | int | - | YES | - | 令牌 ID（主鍵、自動遞增） |
| EVoucherID | int | - | YES | - | 電子禮券 ID（FK） |
| Token | varchar | 64 | YES | - | 64 字元安全令牌（SHA-256） |
| ExpiresAt | datetime2 | - | YES | - | 令牌到期時間 |
| IsRevoked | bit | - | YES | - | 是否已撤銷 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨禮券產生自動建立）

**安全機制說明：**
- Token 為 64 字元隨機字串（使用 SHA-256 或類似演算法）
- 每次掃描 QR Code 時驗證 Token 的有效性與到期時間
- IsRevoked 標記可手動撤銷 Token

---

### 3.4 EVoucherRedeemLog

**表格用途：** 記錄電子禮券的兌換事件（掃描時間、兌換狀態等）

**Primary Key：** `RedeemID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| EVoucherID | EVoucher | EVoucherID | FK_EVoucherRedeemLog_EVoucher |
| TokenID | EVoucherToken | TokenID | FK_EVoucherRedeemLog_Token |
| UserID | Users | User_ID | FK_EVoucherRedeemLog_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| RedeemID | int | - | YES | - | 兌換記錄 ID（主鍵、自動遞增） |
| EVoucherID | int | - | YES | - | 電子禮券 ID（FK） |
| TokenID | int | - | NO | - | 令牌 ID（FK，可能為 NULL） |
| UserID | int | - | YES | - | 兌換者使用者 ID（FK） |
| ScannedAt | datetime2 | - | YES | (sysutcdatetime()) | 掃描時間 |
| Status | nvarchar | 20 | YES | - | 兌換狀態（例如：成功、失敗、已過期、已撤銷） |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨兌換事件產生）

**Status 欄位可能值：**
- `Success` - 兌換成功
- `Failed` - 兌換失敗
- `Expired` - 禮券已過期
- `Revoked` - 令牌已撤銷
- `AlreadyUsed` - 禮券已使用

---

## 四、Sign-In System (簽到系統)

### 4.1 SignInRule

**表格用途：** 設定每日簽到的獎勵規則（點數、經驗值、優惠券）

**Primary Key：** `Id`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 | 特殊說明 |
|---------|---------|---------|---------|----------|
| CouponTypeCode | CouponType | Name | FK_SignInRule_CouponType_Name | 關聯至 CouponType.Name（非典型 FK） |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| Id | int | - | YES | - | 規則 ID（主鍵、自動遞增） |
| SignInDay | int | - | YES | - | 簽到天數（1-30） |
| Points | int | - | YES | - | 贈送點數 |
| Experience | int | - | YES | - | 贈送經驗值 |
| HasCoupon | bit | - | YES | ((0)) | 是否贈送優惠券 |
| CouponTypeCode | nvarchar | 50 | NO | - | 優惠券類型代碼（FK to CouponType.Name） |
| IsActive | bit | - | YES | ((1)) | 是否啟用 |
| CreatedAt | datetime2 | - | YES | (sysutcdatetime()) | 建立時間 |
| UpdatedAt | datetime2 | - | NO | - | 更新時間 |
| Description | nvarchar | 255 | NO | - | 規則描述 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料範例：**

| Id | SignInDay | Points | Experience | HasCoupon | CouponTypeCode | Description |
|----|-----------|--------|------------|-----------|----------------|-------------|
| 1 | 1 | 20 | 0 | 0 | NULL | 第 1 天簽到獎勵 |
| 2 | 2 | 20 | 0 | 0 | NULL | 第 2 天簽到獎勵 |
| 3 | 3 | 20 | 0 | 0 | NULL | 第 3 天簽到獎勵 |
| 4 | 4 | 20 | 0 | 0 | NULL | 第 4 天簽到獎勵 |
| 5 | 5 | 20 | 0 | 0 | NULL | 第 5 天簽到獎勵 |
| 6 | 6 | 30 | 200 | 0 | NULL | 第 6 天簽到獎勵 |
| 7 | 7 | 70 | 500 | 0 | NULL | 第 7 天簽到獎勵 + 週獎勵 |
| 10 | 30 | 200 | 2000 | 1 | MONTH_BONUS | 連續簽到 30 天獎勵（含大禮包） |
| 11 | 14 | 0 | 0 | 1 | TWO_WEEK_BONUS | 連續簽到 14 天獎勵（僅優惠券） |
| 12 | 21 | 0 | 0 | 1 | THREE_WEEK_BONUS | 連續簽到 21 天獎勵（僅優惠券） |

**特殊說明：**
- SignInDay 可以不連續（例如：設定第 1, 2, 3, 6, 7, 14, 30 天的獎勵）
- 當 HasCoupon = 1 時，CouponTypeCode 必須有值
- CouponTypeCode 關聯至 CouponType.Name（而非 CouponTypeID），這是非典型的 FK 設計

---

### 4.2 UserSignInStats

**表格用途：** 記錄使用者每日簽到記錄與獲得的獎勵

**Primary Key：** `LogID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| UserID | Users | User_ID | FK_UserSignInStats_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| LogID | int | - | YES | - | 記錄 ID（主鍵、自動遞增） |
| SignTime | datetime2 | - | YES | (sysutcdatetime()) | 簽到時間 |
| UserID | int | - | YES | - | 使用者 ID（FK） |
| PointsGained | int | - | YES | - | 獲得點數 |
| PointsGainedTime | datetime2 | - | YES | (sysutcdatetime()) | 點數獲得時間 |
| ExpGained | int | - | YES | - | 獲得經驗值 |
| ExpGainedTime | datetime2 | - | YES | (sysutcdatetime()) | 經驗值獲得時間 |
| CouponGained | nvarchar | 50 | YES | - | 獲得的優惠券代碼 |
| CouponGainedTime | datetime2 | - | YES | (sysutcdatetime()) | 優惠券獲得時間 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨用戶簽到產生）

**業務邏輯說明：**
- 每位使用者每天只能簽到一次（由應用層控制）
- SignTime 記錄簽到的實際時間
- PointsGained, ExpGained, CouponGained 由 SignInRule 決定
- 連續簽到邏輯由應用層計算（檢查前一天是否有簽到記錄）

---

## 五、Pet System (寵物系統)

### 5.1 Pet

**表格用途：** 儲存寵物系統的核心資料（屬性、外觀、等級、經驗值等）

**Primary Key：** `PetID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| UserID | Users | User_ID | FK_Pet_Users |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| PetID | int | - | YES | - | 寵物 ID（主鍵、自動遞增） |
| UserID | int | - | YES | - | 擁有者使用者 ID（FK） |
| PetName | nvarchar | 50 | YES | - | 寵物名稱 |
| Level | int | - | YES | - | 寵物等級 |
| LevelUpTime | datetime2 | - | YES | (sysutcdatetime()) | 最後升級時間 |
| Experience | int | - | YES | - | 總經驗值（已棄用，請使用 CurrentExperience） |
| Hunger | int | - | YES | - | 飢餓值（0-100） |
| Mood | int | - | YES | - | 心情值（0-100） |
| Stamina | int | - | YES | - | 體力值（0-100） |
| Cleanliness | int | - | YES | - | 清潔度（0-100） |
| Health | int | - | YES | - | 健康值（0-100） |
| SkinColor | varchar | 10 | YES | - | 膚色代碼（例如：#FFFFFF） |
| SkinColorChangedTime | datetime2 | - | YES | - | 膚色更換時間 |
| BackgroundColor | nvarchar | 20 | YES | - | 背景代碼（例如：BG001） |
| BackgroundColorChangedTime | datetime2 | - | YES | - | 背景更換時間 |
| PointsChanged_SkinColor | int | - | YES | - | 更換膚色消費的點數 |
| PointsChanged_BackgroundColor | int | - | YES | - | 更換背景消費的點數 |
| PointsGained_LevelUp | int | - | YES | - | 最後一次升級獲得的點數 |
| PointsGainedTime_LevelUp | datetime2 | - | YES | (sysutcdatetime()) | 升級獎勵點數獲得時間 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |
| CurrentExperience | int | - | YES | ((0)) | 當前等級經驗值 |
| ExperienceToNextLevel | int | - | NO | - | 升級所需經驗值 |
| TotalPointsGained_LevelUp | int | - | NO | ((0)) | 歷史升級總獎勵點數 |

**種子資料：** 無預設種子資料（隨用戶建立寵物時產生）

**屬性值說明：**
- 五項屬性（Hunger, Mood, Stamina, Cleanliness, Health）數值範圍：0-100
- 屬性會隨時間自然衰減（由 PetDailyDecayService 處理）
- 玩小遊戲會影響屬性值（記錄於 MiniGame 表的 Delta 欄位）

**經驗值系統說明：**
- `Experience` 欄位已棄用（保留以相容舊資料）
- `CurrentExperience` 為當前等級的經驗值（例如：Level 5 有 350 Exp，下一級需 500 Exp）
- 升級時 CurrentExperience 重置，Level 增加
- `ExperienceToNextLevel` 由應用層計算（根據 PetLevelExperienceSetting 表）

---

### 5.2 PetSkinColorCostSettings

**表格用途：** 設定寵物可選擇的膚色選項、稀有度、價格

**Primary Key：** `SettingId`

**Foreign Keys：** 無

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| SettingId | int | - | YES | - | 設定 ID（主鍵、自動遞增） |
| ColorCode | varchar | 10 | YES | - | 顏色代碼（例如：#FFFFFF） |
| ColorName | nvarchar | 50 | YES | - | 顏色名稱（例如：純白） |
| PointsCost | int | - | YES | ((2000)) | 兌換所需點數 |
| Rarity | nvarchar | 20 | YES | (N'普通') | 稀有度（普通/稀有/傳說/限定/活動） |
| Description | nvarchar | 500 | NO | - | 顏色描述 |
| PreviewImagePath | nvarchar | 500 | NO | - | 預覽圖片路徑 |
| ColorHex | varchar | 7 | NO | - | Hex 顏色碼（例如：#FFFFFF） |
| IsActive | bit | - | YES | ((1)) | 是否啟用 |
| DisplayOrder | int | - | YES | ((0)) | 顯示順序 |
| IsFree | bit | - | YES | ((0)) | 是否免費 |
| IsLimitedEdition | bit | - | YES | ((0)) | 是否限定版 |
| AvailableFrom | datetime2 | - | NO | - | 開放時間（限定版用） |
| AvailableUntil | datetime2 | - | NO | - | 結束時間（限定版用） |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |
| CreatedAt | datetime2 | - | YES | (sysutcdatetime()) | 建立時間 |
| UpdatedAt | datetime2 | - | NO | - | 更新時間 |
| UpdatedBy | int | - | NO | - | 更新者 ID |

**種子資料範例（前 11 筆）：**

| ID | ColorCode | ColorName | PointsCost | Rarity | IsFree | IsActive | Description |
|----|-----------|-----------|------------|--------|--------|----------|-------------|
| 1 | #FFFFFF | 純白 | 0 | 普通 | 1 | 1 | 經典的純白，預設可用 |
| 2 | #000000 | 黑色 | 0 | 普通 | 1 | 1 | 神秘的黑色，預設可用 |
| 3 | #FF0000 | 紅色 | 0 | 普通 | 1 | 1 | 熱情的紅色，預設可用 |
| 4 | #FFA500 | 橘色 | 2000 | 普通 | 0 | 1 | 溫暖明亮的橘色 |
| 5 | #FFFF00 | 黃色 | 2000 | 普通 | 0 | 1 | 陽光明媚的黃色 |
| 6 | #008000 | 綠色 | 2000 | 普通 | 0 | 1 | 生命清新的綠色 |
| 7 | #00FFFF | 青色 | 2000 | 普通 | 0 | 1 | 清爽透明的青色 |
| 8 | #0000FF | 藍色 | 2000 | 普通 | 0 | 1 | 深邃如海的藍色 |
| 9 | #800080 | 紫色 | 3500 | 稀有 | 0 | 1 | 高貴優雅的紫色 |
| 10 | #6F4E37 | 咖啡色 | 3500 | 稀有 | 0 | 1 | 溫暖大地的咖啡色 |
| 11 | #6EFE19 | 草綠色 | 2000 | 活動 | 0 | 0 | 限時春季活動色彩（設定下架） |

**稀有度分類：**
- `普通` - 常規顏色（2000 點數）
- `稀有` - 較難取得（3500 點數）
- `傳說` - 極稀有（更高點數）
- `限定` - 限時開放
- `活動` - 活動期間限定

---

### 5.3 PetLevelRewardSettings

**表格用途：** 設定寵物升級時依據等級範圍給予的點數獎勵

**Primary Key：** `SettingId`

**Foreign Keys：** 無

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| SettingId | int | - | YES | - | 設定 ID（主鍵、自動遞增） |
| LevelRangeStart | int | - | YES | - | 等級範圍起始（例如：1） |
| LevelRangeEnd | int | - | YES | - | 等級範圍結束（例如：10） |
| PointsReward | int | - | YES | - | 獎勵點數 |
| Description | nvarchar | 500 | NO | - | 獎勵說明 |
| IsActive | bit | - | YES | ((1)) | 是否啟用 |
| DisplayOrder | int | - | YES | ((0)) | 顯示順序 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |
| CreatedAt | datetime2 | - | YES | (sysutcdatetime()) | 建立時間 |
| UpdatedAt | datetime2 | - | NO | - | 更新時間 |
| UpdatedBy | int | - | NO | - | 更新者 ID |

**種子資料範例（全部 20 筆）：**

| ID | LevelRangeStart | LevelRangeEnd | PointsReward | Description |
|----|-----------------|---------------|--------------|-------------|
| 1 | 1 | 10 | 10 | 新手獎勵（Level 1-10） |
| 2 | 11 | 20 | 20 | 初級獎勵（Level 11-20） |
| 3 | 21 | 30 | 30 | 中級獎勵（Level 21-30） |
| 4 | 31 | 40 | 40 | 高級獎勵（Level 31-40） |
| 5 | 41 | 50 | 50 | 專家獎勵（Level 41-50） |
| 6 | 51 | 60 | 60 | 精英獎勵（Level 51-60） |
| 7 | 61 | 70 | 70 | 大師獎勵（Level 61-70） |
| 8 | 71 | 80 | 80 | 宗師獎勵（Level 71-80） |
| 9 | 81 | 90 | 90 | 傳奇獎勵（Level 81-90） |
| 10 | 91 | 100 | 100 | 宗師獎勵（Level 91-100） |
| 11 | 101 | 110 | 110 | 神話獎勵（Level 101-110） |
| 12 | 111 | 120 | 120 | 永恆獎勵（Level 111-120） |
| 13 | 121 | 130 | 130 | 至尊獎勵（Level 121-130） |
| 14 | 131 | 140 | 140 | 無極獎勵（Level 131-140） |
| 15 | 141 | 150 | 150 | 超凡獎勵（Level 141-150） |
| 16 | 151 | 160 | 160 | 聖境獎勵（Level 151-160） |
| 17 | 161 | 170 | 170 | 天界獎勵（Level 161-170） |
| 18 | 171 | 180 | 180 | 神域獎勵（Level 171-180） |
| 19 | 181 | 190 | 190 | 混沌獎勵（Level 181-190） |
| 20 | 191 | 200 | 200 | 終極獎勵（Level 191-200） |

**業務邏輯說明：**
- 等級範圍不可重疊
- 寵物升級時，根據當前等級查詢對應的 PointsReward
- 例如：寵物從 Level 25 升級到 Level 26，會獲得 30 點數（因為屬於 21-30 範圍）

---

### 5.4 PetBackgroundCostSettings

**表格用途：** 設定寵物可選擇的背景圖案選項、稀有度、價格

**Primary Key：** `SettingId`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| UpdatedBy | ManagerData | Manager_Id | FK_PetBackgroundCostSettings_UpdatedBy_Manager |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| SettingId | int | - | YES | - | 設定 ID（主鍵、自動遞增） |
| BackgroundCode | nvarchar | 50 | YES | - | 背景代碼（例如：BG001） |
| BackgroundName | nvarchar | 100 | YES | - | 背景名稱 |
| PointsCost | int | - | YES | - | 兌換所需點數 |
| Description | nvarchar | 500 | NO | - | 背景描述 |
| PreviewImagePath | nvarchar | 200 | NO | - | 預覽圖片路徑 |
| IsActive | bit | - | YES | ((1)) | 是否啟用 |
| DisplayOrder | int | - | NO | ((0)) | 顯示順序 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |
| CreatedAt | datetime2 | - | YES | (sysutcdatetime()) | 建立時間 |
| UpdatedAt | datetime2 | - | NO | - | 更新時間 |
| UpdatedBy | int | - | NO | - | 更新者管理員 ID（FK） |
| Rarity | nvarchar | 20 | NO | - | 稀有度 |

**種子資料範例（前 11 筆）：**

| ID | BackgroundCode | BackgroundName | PointsCost | Rarity | IsActive | PreviewImagePath |
|----|----------------|----------------|------------|--------|----------|------------------|
| 19 | BG001 | 萬聖夜南瓜 | 0 | 普通 | 1 | /images/backgrounds/halloween-pumpkin.jpg |
| 20 | BG002 | 彩繪玻璃教堂 | 0 | 普通 | 1 | /images/backgrounds/stained-glass-church.jpg |
| 21 | BG003 | 珊瑚礁海灘 | 0 | 普通 | 1 | /images/backgrounds/coral-beach.jpg |
| 22 | BG004 | 清新森林 | 2000 | 普通 | 1 | /images/backgrounds/fresh-forest.jpg |
| 23 | BG005 | 熱帶雨林瀑布 | 2500 | 普通 | 1 | /images/backgrounds/rainforest-waterfall.jpg |
| 24 | BG006 | 清晨教室 | 3000 | 普通 | 1 | /images/backgrounds/morning-classroom.jpg |
| 25 | BG007 | 霓虹都市夜景 | 3500 | 普通 | 1 | /images/backgrounds/neon-skyline.jpg |
| 26 | BG008 | 極光雪原 | 4000 | 普通 | 1 | /images/backgrounds/aurora-snowfield.jpg |
| 27 | BG009 | 魔法圖書館 | 4500 | 稀有 | 1 | /images/backgrounds/magic-library.jpg |
| 28 | BG010 | 地獄熔岩 | 6000 | 稀有 | 1 | /images/backgrounds/hell-lava.jpg |
| 29 | BG011 | 蒸汽工房 | 2000 | 活動 | 0 | /images/backgrounds/steam-workshop.jpg |

**特殊說明：**
- UpdatedBy 欄位關聯至 ManagerData，用於稽核追蹤（記錄哪位管理員修改了設定）
- IsActive = 0 表示該背景已下架（例如：BG011 蒸汽工房為限時活動背景）

---

## 六、Mini-Game System (小遊戲系統)

### 6.1 MiniGame

**表格用途：** 記錄使用者玩小遊戲的每一場遊玩記錄（難度、結果、獎勵、寵物屬性變化）

**Primary Key：** `PlayID`

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 |
|---------|---------|---------|---------|
| UserID | Users | User_ID | FK_MiniGame_Users |
| PetID | Pet | PetID | FK_MiniGame_Pet |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| PlayID | int | - | YES | - | 遊戲記錄 ID（主鍵、自動遞增） |
| UserID | int | - | YES | - | 玩家使用者 ID（FK） |
| PetID | int | - | YES | - | 參與的寵物 ID（FK） |
| Level | int | - | YES | - | 遊戲難度等級 |
| MonsterCount | int | - | YES | - | 怪物數量 |
| SpeedMultiplier | decimal | (3,2) | YES | - | 速度倍率（例如：1.5） |
| Result | nvarchar | 20 | YES | - | 遊戲結果（Win/Lose/Draw） |
| ExpGained | int | - | YES | - | 獲得經驗值 |
| ExpGainedTime | datetime2 | - | YES | (sysutcdatetime()) | 經驗值獲得時間 |
| PointsGained | int | - | YES | - | 獲得點數 |
| PointsGainedTime | datetime2 | - | YES | (sysutcdatetime()) | 點數獲得時間 |
| CouponGained | nvarchar | 50 | YES | - | 獲得的優惠券代碼 |
| CouponGainedTime | datetime2 | - | YES | (sysutcdatetime()) | 優惠券獲得時間 |
| HungerDelta | int | - | YES | - | 飢餓值變化量（負數表示減少） |
| MoodDelta | int | - | YES | - | 心情值變化量 |
| StaminaDelta | int | - | YES | - | 體力值變化量 |
| CleanlinessDelta | int | - | YES | - | 清潔度變化量 |
| StartTime | datetime2 | - | YES | (sysutcdatetime()) | 遊戲開始時間 |
| EndTime | datetime2 | - | NO | (sysutcdatetime()) | 遊戲結束時間 |
| Aborted | bit | - | YES | - | 是否中途放棄 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（隨遊玩產生）

**Result 欄位可能值：**
- `Win` - 勝利
- `Lose` - 失敗
- `Draw` - 平手
- `Aborted` - 中途放棄

**屬性 Delta 欄位說明：**
- 正數表示屬性增加
- 負數表示屬性減少
- 例如：HungerDelta = -10 表示飢餓值減少 10（更餓了）
- 例如：MoodDelta = +5 表示心情值增加 5（更開心了）

**業務邏輯說明：**
- 遊戲難度由 Level, MonsterCount, SpeedMultiplier 三個參數決定
- 難度越高，獎勵（ExpGained, PointsGained）越高
- 遊戲會消耗寵物體力（StaminaDelta 通常為負數）
- 勝利可能提升心情（MoodDelta 為正數），失敗可能降低心情

---

## 七、Manager/Admin System (管理員系統)

### 7.1 ManagerData

**表格用途：** 儲存管理員帳號資訊（帳號、密碼、Email、鎖定機制等）

**Primary Key：** `Manager_Id`

**Foreign Keys：** 無

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| Manager_Id | int | - | YES | - | 管理員 ID（主鍵、自動遞增） |
| Manager_Name | nvarchar | 30 | NO | - | 管理員姓名 |
| Manager_Account | varchar | 30 | NO | - | 管理員帳號 |
| Manager_Password | nvarchar | 200 | NO | - | 管理員密碼（Hash 加密） |
| Administrator_registration_date | datetime2 | - | NO | - | 管理員註冊日期 |
| Manager_Email | nvarchar | 255 | YES | - | 管理員 Email |
| Manager_EmailConfirmed | bit | - | YES | ((0)) | Email 是否已驗證 |
| Manager_AccessFailedCount | int | - | YES | ((0)) | 登入失敗次數 |
| Manager_LockoutEnabled | bit | - | YES | ((1)) | 是否啟用帳號鎖定機制 |
| Manager_LockoutEnd | datetime2 | - | NO | - | 帳號鎖定結束時間 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（由系統管理員手動建立）

**帳號鎖定機制說明：**
- Manager_AccessFailedCount 記錄連續登入失敗次數
- 失敗 5 次後，Manager_LockoutEnd 設定為 15 分鐘後
- 鎖定期間內無法登入
- 成功登入後，Manager_AccessFailedCount 重置為 0

**密碼安全說明：**
- Manager_Password 必須儲存 Hash 值（例如：BCrypt, SHA-256）
- 絕不儲存明文密碼

---

### 7.2 ManagerRole

**表格用途：** 關聯表（Junction Table），實現管理員與角色的多對多關係（RBAC）

**Primary Key：** Composite (`Manager_Id`, `ManagerRole_Id`)

**Foreign Keys：**
| FK 欄位 | 關聯表格 | 關聯欄位 | FK 名稱 | 備註 |
|---------|---------|---------|---------|------|
| Manager_Id | ManagerData | Manager_Id | FK__ManagerRo__Manag__0BE6BFCF | 重複 FK（舊版） |
| Manager_Id | ManagerData | Manager_Id | FK__ManagerRo__Manag__57A801BA | 重複 FK（新版） |
| ManagerRole_Id | ManagerRolePermission | ManagerRole_Id | FK__ManagerRo__Manag__0CDAE408 | 重複 FK（舊版） |
| ManagerRole_Id | ManagerRolePermission | ManagerRole_Id | FK__ManagerRo__Manag__589C25F3 | 重複 FK（新版） |

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| Manager_Id | int | - | YES | - | 管理員 ID（主鍵、FK） |
| ManagerRole_Id | int | - | YES | - | 角色 ID（主鍵、FK） |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（由系統管理員手動分配角色）

**RBAC 說明：**
- 一位管理員可以擁有多個角色
- 一個角色可以分配給多位管理員
- 例如：Manager_Id=1 可以同時擁有 ManagerRole_Id=1（超級管理員）和 ManagerRole_Id=3（客服）

**重複 FK 警告：**
此表格存在重複的 FK 約束（可能因多次 Migration 產生）。建議管理員檢查並移除重複的 FK 約束，保留其中一組即可。

---

### 7.3 ManagerRolePermission

**表格用途：** 定義每個角色的權限集合（使用布林欄位表示各項權限）

**Primary Key：** `ManagerRole_Id`

**Foreign Keys：** 無

**欄位定義：**

| 欄位名稱 | 資料型別 | 長度 | 必填 | 預設值 | 說明 |
|---------|---------|------|------|--------|------|
| ManagerRole_Id | int | - | YES | - | 角色 ID（主鍵、自動遞增） |
| role_name | nvarchar | 50 | YES | - | 角色名稱（例如：超級管理員、客服） |
| AdministratorPrivilegesManagement | bit | - | NO | - | 管理員權限管理（Super Admin） |
| UserStatusManagement | bit | - | NO | - | 使用者帳號管理 |
| ShoppingPermissionManagement | bit | - | NO | - | 購物權限管理（錢包/優惠券） |
| MessagePermissionManagement | bit | - | NO | - | 訊息系統管理 |
| Pet_Rights_Management | bit | - | NO | - | 寵物系統管理 |
| customer_service | bit | - | NO | - | 客服角色權限 |
| IsDeleted | bit | - | YES | ((0)) | 軟刪除標記 |
| DeletedAt | datetime2 | - | NO | - | 刪除時間 |
| DeletedBy | int | - | NO | - | 刪除者 ID |
| DeleteReason | nvarchar | 500 | NO | - | 刪除原因 |

**種子資料：** 無預設種子資料（由系統管理員手動建立角色）

**權限欄位說明：**

| 權限欄位 | 說明 | 適用功能 |
|---------|------|---------|
| AdministratorPrivilegesManagement | 超級管理員權限 | 管理其他管理員帳號、分配角色 |
| UserStatusManagement | 使用者管理權限 | 查看/編輯/停用使用者帳號 |
| ShoppingPermissionManagement | 購物系統管理 | 管理錢包、優惠券、電子禮券 |
| MessagePermissionManagement | 訊息系統管理 | 查看/發送/管理系統訊息 |
| Pet_Rights_Management | 寵物系統管理 | 管理寵物設定、外觀選項、升級規則 |
| customer_service | 客服權限 | 處理客戶服務相關事務 |

**角色範例：**

| role_name | AdministratorPrivilegesManagement | UserStatusManagement | ShoppingPermissionManagement | Pet_Rights_Management | customer_service |
|-----------|-----------------------------------|----------------------|------------------------------|----------------------|------------------|
| 超級管理員 | 1 | 1 | 1 | 1 | 1 |
| 使用者管理員 | 0 | 1 | 0 | 0 | 0 |
| 購物管理員 | 0 | 0 | 1 | 0 | 0 |
| 寵物系統管理員 | 0 | 0 | 0 | 1 | 0 |
| 客服人員 | 0 | 1 | 0 | 0 | 1 |

---

# 附錄

## A. 常見設計模式說明

### A.1 Soft Delete Pattern (軟刪除模式)

**適用範圍：** 所有 18 張表格

**欄位組成：**
- `IsDeleted` (bit, default 0) - 軟刪除標記
- `DeletedAt` (datetime2, nullable) - 刪除時間
- `DeletedBy` (int, nullable) - 刪除者 ID
- `DeleteReason` (nvarchar(500), nullable) - 刪除原因

**業務邏輯：**
1. 刪除資料時，設定 `IsDeleted = 1`
2. 記錄 `DeletedAt` 為當前時間（UTC）
3. 記錄 `DeletedBy` 為執行刪除的管理員 ID
4. 記錄 `DeleteReason` 為刪除原因（例如：「使用者要求」、「資料錯誤」）
5. 查詢時，預設過濾 `WHERE IsDeleted = 0`

**優點：**
- 可復原誤刪的資料
- 保留完整的歷史記錄
- 符合稽核要求

**注意事項：**
- 絕不執行 `DELETE` 指令（硬刪除）
- 定期清理過期的軟刪除資料（例如：1 年後）

---

### A.2 Audit Tracking Pattern (稽核追蹤模式)

**適用範圍：** 部分表格（例如：PetSkinColorCostSettings, PetLevelRewardSettings, PetBackgroundCostSettings）

**欄位組成：**
- `CreatedAt` (datetime2, default sysutcdatetime()) - 建立時間
- `UpdatedAt` (datetime2, nullable) - 更新時間
- `UpdatedBy` (int, nullable, FK to ManagerData) - 更新者管理員 ID

**業務邏輯：**
1. 新增資料時，`CreatedAt` 自動填入當前 UTC 時間
2. 更新資料時，設定 `UpdatedAt` 為當前 UTC 時間
3. 記錄 `UpdatedBy` 為執行更新的管理員 ID

**優點：**
- 追蹤資料變更歷史
- 了解誰在何時修改了設定
- 符合稽核與合規要求

---

### A.3 Default Timestamp Pattern (預設時間戳模式)

**適用範圍：** 大部分表格

**常見欄位：**
- `CreatedAt` - 預設值：`(sysutcdatetime())`
- `SignTime` - 預設值：`(sysutcdatetime())`
- `ChangeTime` - 預設值：`(sysutcdatetime())`
- `AcquiredTime` - 預設值：`(sysutcdatetime())`

**優點：**
- 自動記錄時間，無需應用層手動設定
- 使用 UTC 時間，避免時區問題
- 提高資料一致性

---

## B. 未來 AI 使用指引

### B.1 資料庫結構理解要點

1. **表格關聯理解：**
   - 所有 MiniGame Area 表格都關聯至主系統的 `Users` 表（透過 User_ID）
   - 寵物系統與遊戲系統緊密關聯（Pet ← MiniGame）
   - 簽到系統與優惠券系統有業務關聯（SignInRule → CouponType）

2. **業務邏輯重點：**
   - 錢包系統：User_Wallet（餘額） + WalletHistory（交易記錄）
   - 優惠券/禮券：Type 表定義範本，Instance 表記錄發放實例
   - 寵物系統：5 項屬性會自然衰減，需定期維護
   - 簽到系統：連續簽到邏輯由應用層計算

3. **資料完整性：**
   - 所有表格都使用軟刪除，絕不硬刪除
   - 時間戳欄位使用 UTC 時間
   - Foreign Key 關係已建立，保證資料一致性

### B.2 常見查詢場景

**場景 1：查詢使用者的錢包餘額與最近 10 筆交易記錄**
```sql
-- 錢包餘額
SELECT User_Point FROM User_Wallet WHERE User_Id = @UserId AND IsDeleted = 0;

-- 最近 10 筆交易
SELECT TOP 10 * FROM WalletHistory
WHERE UserID = @UserId AND IsDeleted = 0
ORDER BY ChangeTime DESC;
```

**場景 2：查詢使用者可兌換的優惠券類型（有足夠點數且在有效期內）**
```sql
SELECT ct.*
FROM CouponType ct
INNER JOIN User_Wallet uw ON uw.User_Point >= ct.PointsCost
WHERE uw.User_Id = @UserId
  AND ct.ValidFrom <= SYSUTCDATETIME()
  AND ct.ValidTo >= SYSUTCDATETIME()
  AND ct.IsDeleted = 0
  AND uw.IsDeleted = 0;
```

**場景 3：查詢使用者的寵物資訊與當前等級的升級所需經驗值**
```sql
SELECT p.*, plrs.PointsReward
FROM Pet p
LEFT JOIN PetLevelRewardSettings plrs
  ON p.Level BETWEEN plrs.LevelRangeStart AND plrs.LevelRangeEnd
  AND plrs.IsActive = 1 AND plrs.IsDeleted = 0
WHERE p.UserID = @UserId AND p.IsDeleted = 0;
```

**場景 4：查詢使用者本月的簽到記錄**
```sql
SELECT * FROM UserSignInStats
WHERE UserID = @UserId
  AND MONTH(SignTime) = MONTH(SYSUTCDATETIME())
  AND YEAR(SignTime) = YEAR(SYSUTCDATETIME())
  AND IsDeleted = 0
ORDER BY SignTime DESC;
```

### B.3 效能優化建議

1. **索引建議：**
   - `User_Wallet.User_Id` - 已是 Primary Key（自動索引）
   - `WalletHistory.UserID` - 建議建立索引（常用於查詢使用者交易記錄）
   - `WalletHistory.ChangeTime` - 建議建立索引（常用於時間範圍查詢）
   - `Pet.UserID` - 建議建立索引
   - `MiniGame.UserID` - 建議建立索引
   - `UserSignInStats.UserID + SignTime` - 建議建立複合索引

2. **查詢優化：**
   - 查詢時永遠加上 `WHERE IsDeleted = 0`（避免撈出軟刪除資料）
   - 使用 `SYSUTCDATETIME()` 而非 `GETDATE()`（保持 UTC 時間一致性）
   - 避免 `SELECT *`，只選取需要的欄位

3. **交易安全：**
   - 錢包點數變動必須使用 Transaction（同時更新 User_Wallet 與 WalletHistory）
   - 優惠券兌換必須使用 Transaction（檢查庫存 → 扣點數 → 建立 Coupon）

---

## C. 重要注意事項與限制

### C.1 禁止事項

1. **禁止使用 Entity Framework Migrations：**
   - MiniGame Area 的所有表格都是透過 SSMS 手動管理
   - 絕不使用 EF Migrations 修改資料庫結構
   - 如需修改，請直接使用 SQL Script

2. **禁止硬刪除：**
   - 絕不執行 `DELETE FROM` 指令
   - 所有刪除操作都必須使用軟刪除（UPDATE IsDeleted = 1）

3. **禁止儲存明文密碼：**
   - ManagerData.Manager_Password 必須儲存 Hash 值
   - 使用 BCrypt, SHA-256 或其他安全演算法

### C.2 編碼要求

1. **所有檔案必須使用 UTF-8 with BOM：**
   - SQL Script 檔案
   - 文件檔案（.md, .txt）
   - CSV 檔案

2. **中文字元處理：**
   - 資料庫欄位使用 `nvarchar` 儲存中文
   - 查詢時使用 `N'中文'` 前綴（例如：`WHERE ColorName = N'純白'`）

### C.3 資料一致性要求

1. **Foreign Key 檢查：**
   - 新增資料前，必須確認 FK 關聯的資料存在
   - 例如：建立 Coupon 前，確認 CouponTypeID 存在

2. **業務規則檢查：**
   - 兌換優惠券前，檢查使用者點數是否足夠
   - 簽到前，檢查當日是否已簽到
   - 更換寵物外觀前，檢查點數是否足夠

3. **時間處理：**
   - 所有時間欄位使用 UTC 時間
   - 顯示時再轉換為使用者本地時區

---

## D. 資料庫連線資訊

**連線字串（sqlcmd）：**
```bash
sqlcmd -S ".\SQLEXPRESS" -d GameSpacedatabase -E
```

**連線字串（C# ADO.NET）：**
```
Server=DESKTOP-8HQIS1S\SQLEXPRESS;Database=GameSpacedatabase;Trusted_Connection=True;TrustServerCertificate=True;
```

**連線字串（Entity Framework）：**
```
Server=DESKTOP-8HQIS1S\SQLEXPRESS;Database=GameSpacedatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;
```

---

## E. 版本歷史

| 版本 | 日期 | 修改者 | 修改內容 |
|------|------|--------|---------|
| 1.0 | 2025-10-21 | Claude AI | 初始版本建立 |

---

**文件結束**

此文件由 Claude AI 基於實際資料庫結構自動生成。如有疑問或需要更新，請聯繫系統管理員。
