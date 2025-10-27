# 資料庫結構完整摘要（實際查詢結果）
**查詢日期**: 2025-10-27
**資料庫伺服器**: DESKTOP-8HQIS1S\SQLEXPRESS
**資料庫**: GameSpaceDatabase
**SQL Server版本**: Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 Express Edition

## 所有表格清單（20張）

### MiniGame Area 主要表格（16張）
1. Coupon - 優惠券實例
2. CouponType - 優惠券類型定義
3. EVoucher - 電子禮券實例
4. EVoucherRedeemLog - 電子禮券核銷記錄
5. EVoucherToken - 電子禮券核銷憑證
6. EVoucherType - 電子禮券類型定義
7. MiniGame - 小遊戲記錄
8. Pet - 寵物系統資料
9. PetBackgroundCostSettings - 寵物背景價格設定
10. PetLevelRewardSettings - 寵物升級獎勵規則
11. PetSkinColorCostSettings - 寵物膚色價格設定
12. SignInRule - 簽到規則設定
13. SystemSettings - 系統設定
14. User_Wallet - 會員錢包
15. UserSignInStats - 簽到統計記錄
16. WalletHistory - 錢包異動歷史

### 使用者/權限相關表格（4張）
17. ManagerData - 管理員基本資料
18. ManagerRole - 管理員角色分配
19. ManagerRolePermission - 角色權限定義
20. Users - 使用者基本資料

---

## 詳細表格結構

### 1. Coupon（優惠券實例）
**主鍵**: CouponID (int, IDENTITY)
**外鍵**:
- CouponTypeID → CouponType.CouponTypeID (NO_ACTION)
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- CouponID (int, NOT NULL, IDENTITY) - PK
- CouponCode (nvarchar(50), NOT NULL) - UNIQUE
- CouponTypeID (int, NOT NULL)
- UserID (int, NOT NULL)
- IsUsed (bit, NOT NULL, DEFAULT 0)
- AcquiredTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UsedTime (datetime2(7), NULL, DEFAULT sysutcdatetime())
- UsedInOrderID (int, NULL)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**CHECK Constraints**:
- CK_Coupon_IsUsed: [IsUsed]=(1) OR [IsUsed]=(0)
- CK_Coupon_UsedFields: ([IsUsed]=(0) AND [UsedTime] IS NULL AND [UsedInOrderID] IS NULL) OR ([IsUsed]=(1) AND [UsedTime] IS NOT NULL AND [UsedInOrderID] IS NOT NULL)

**索引**:
- UQ_Coupon_CouponCode (UNIQUE) on CouponCode
- IX_Coupon_IsDeleted on IsDeleted
- IX_Coupon_user_used on (UserID, IsUsed, AcquiredTime)

---

### 2. CouponType（優惠券類型定義）
**主鍵**: CouponTypeID (int, IDENTITY)

**欄位**:
- CouponTypeID (int, NOT NULL, IDENTITY) - PK
- Name (nvarchar(50), NOT NULL) - UNIQUE
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

**CHECK Constraints**:
- CK_CouponType_DiscountType: upper(ltrim(rtrim([DiscountType])))=N'PERCENT' OR upper(ltrim(rtrim([DiscountType])))=N'AMOUNT'
- CK_CouponType_ValidRange: [ValidFrom]<=[ValidTo]

**索引**:
- UQ_CouponType_Name (UNIQUE) on Name
- IX_CouponType_IsDeleted on IsDeleted

---

### 3. EVoucher（電子禮券實例）
**主鍵**: EVoucherID (int, IDENTITY)
**外鍵**:
- EVoucherTypeID → EVoucherType.EVoucherTypeID (NO_ACTION)
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- EVoucherID (int, NOT NULL, IDENTITY) - PK
- EVoucherCode (nvarchar(50), NOT NULL) - UNIQUE
- EVoucherTypeID (int, NOT NULL)
- UserID (int, NOT NULL)
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

---

### 4. EVoucherRedeemLog（電子禮券核銷記錄）
**主鍵**: RedeemID (int, IDENTITY)
**外鍵**:
- EVoucherID → EVoucher.EVoucherID (CASCADE)
- TokenID → EVoucherToken.TokenID (NO_ACTION)
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- RedeemID (int, NOT NULL, IDENTITY) - PK
- EVoucherID (int, NOT NULL)
- TokenID (int, NULL)
- UserID (int, NOT NULL)
- ScannedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- Status (nvarchar(20), NOT NULL) - 'REVOKED', 'REJECTED', 'EXPIRED', 'ALREADYUSED', 'APPROVED'
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**CHECK Constraints**:
- CK_EVoucherRedeemLog_Status: upper([Status])=N'REVOKED' OR upper([Status])=N'REJECTED' OR upper([Status])=N'EXPIRED' OR upper([Status])=N'ALREADYUSED' OR upper([Status])=N'APPROVED'

---

### 5. EVoucherToken（電子禮券核銷憑證）
**主鍵**: TokenID (int, IDENTITY)
**外鍵**:
- EVoucherID → EVoucher.EVoucherID (NO_ACTION)

**欄位**:
- TokenID (int, NOT NULL, IDENTITY) - PK
- EVoucherID (int, NOT NULL)
- Token (varchar(64), NOT NULL)
- ExpiresAt (datetime2(7), NOT NULL)
- IsRevoked (bit, NOT NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

---

### 6. EVoucherType（電子禮券類型定義）
**主鍵**: EVoucherTypeID (int, IDENTITY)

**欄位**:
- EVoucherTypeID (int, NOT NULL, IDENTITY) - PK
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

---

### 7. MiniGame（小遊戲記錄）
**主鍵**: PlayID (int, IDENTITY)
**外鍵**:
- PetID → Pet.PetID (NO_ACTION)
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- PlayID (int, NOT NULL, IDENTITY) - PK
- UserID (int, NOT NULL)
- PetID (int, NOT NULL)
- Level (int, NOT NULL)
- MonsterCount (int, NOT NULL)
- SpeedMultiplier (decimal(5,2), NOT NULL)
- Result (nvarchar(20), NOT NULL)
- ExpGained (int, NOT NULL)
- ExpGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- PointsGained (int, NOT NULL)
- PointsGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- CouponGained (nvarchar(50), NOT NULL)
- CouponGainedTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- HungerDelta (int, NOT NULL)
- MoodDelta (int, NOT NULL)
- StaminaDelta (int, NOT NULL)
- CleanlinessDelta (int, NOT NULL)
- StartTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- EndTime (datetime2(7), NULL, DEFAULT sysutcdatetime())
- Aborted (bit, NOT NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

---

### 8. Pet（寵物系統資料）
**主鍵**: PetID (int, IDENTITY)
**外鍵**:
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- PetID (int, NOT NULL, IDENTITY) - PK
- UserID (int, NOT NULL)
- PetName (nvarchar(50), NOT NULL)
- Level (int, NOT NULL)
- LevelUpTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- Experience (int, NOT NULL)
- Hunger (int, NOT NULL) - 0-100
- Mood (int, NOT NULL) - 0-100
- Stamina (int, NOT NULL) - 0-100
- Cleanliness (int, NOT NULL) - 0-100
- Health (int, NOT NULL) - 0-100
- SkinColor (varchar(7), NOT NULL)
- SkinColorChangedTime (datetime2(7), NOT NULL)
- BackgroundColor (nvarchar(20), NOT NULL)
- BackgroundColorChangedTime (datetime2(7), NOT NULL)
- PointsChanged_SkinColor (int, NOT NULL)
- PointsChanged_BackgroundColor (int, NOT NULL)
- PointsGained_LevelUp (int, NOT NULL)
- PointsGainedTime_LevelUp (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)
- CurrentExperience (int, NOT NULL, DEFAULT 0)
- ExperienceToNextLevel (int, NULL)
- TotalPointsGained_LevelUp (int, NULL, DEFAULT 0)

**CHECK Constraints**:
- CK_Pet_Hunger: [Hunger]>=(0) AND [Hunger]<=(100)
- CK_Pet_Mood: [Mood]>=(0) AND [Mood]<=(100)
- CK_Pet_Stamina: [Stamina]>=(0) AND [Stamina]<=(100)
- CK_Pet_Cleanliness: [Cleanliness]>=(0) AND [Cleanliness]<=(100)
- CK_Pet_Health: [Health]>=(0) AND [Health]<=(100)

---

### 9. PetBackgroundCostSettings（寵物背景價格設定）
**主鍵**: SettingId (int, IDENTITY)

**欄位**:
- SettingId (int, NOT NULL, IDENTITY) - PK
- BackgroundCode (nvarchar(50), NOT NULL)
- BackgroundName (nvarchar(100), NOT NULL)
- PointsCost (int, NOT NULL) - >= 0
- Description (nvarchar(500), NULL)
- PreviewImagePath (nvarchar(200), NULL)
- IsActive (bit, NOT NULL, DEFAULT 1)
- DisplayOrder (int, NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- UpdatedBy (int, NULL)
- Rarity (nvarchar(20), NULL)

**CHECK Constraints**:
- CK__PetBackgr__Point__541767F8: [PointsCost]>=(0)

---

### 10. PetLevelRewardSettings（寵物升級獎勵規則）
**主鍵**: SettingId (int, IDENTITY)

**欄位**:
- SettingId (int, NOT NULL, IDENTITY) - PK
- LevelRangeStart (int, NOT NULL) - > 0
- LevelRangeEnd (int, NOT NULL) - >= LevelRangeStart
- PointsReward (int, NOT NULL) - 0-999999
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
- CK_PetLevelRewardSettings_LevelRange: [LevelRangeStart]>(0) AND [LevelRangeEnd]>=[LevelRangeStart]
- CK_PetLevelRewardSettings_PointsReward: [PointsReward]>=(0) AND [PointsReward]<=(999999)

---

### 11. PetSkinColorCostSettings（寵物膚色價格設定）
**主鍵**: SettingId (int, IDENTITY)

**欄位**:
- SettingId (int, NOT NULL, IDENTITY) - PK
- ColorCode (varchar(7), NOT NULL) - 格式: #RRGGBB
- ColorName (nvarchar(50), NOT NULL)
- PointsCost (int, NOT NULL, DEFAULT 2000) - >= 0
- Rarity (nvarchar(20), NOT NULL, DEFAULT '普通') - '普通', '稀有', '史詩', '傳說', '限定'
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
- CK_PetSkinColorCostSettings_PointsCost: [PointsCost]>=(0)
- CK_PetSkinColorCostSettings_ColorCode: [ColorCode] like '#%' AND len([ColorCode])>=(4)
- CK_PetSkinColorCostSettings_Rarity: [Rarity]=N'普通' OR [Rarity]=N'稀有' OR [Rarity]=N'史詩' OR [Rarity]=N'傳說' OR [Rarity]=N'限定'

---

### 12. SignInRule（簽到規則設定）
**主鍵**: Id (int, IDENTITY)

**欄位**:
- Id (int, NOT NULL, IDENTITY) - PK
- SignInDay (int, NOT NULL) - 1-365, UNIQUE
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

**CHECK Constraints**:
- CK_SignInRule_DayRange: [SignInDay]>=(1) AND [SignInDay]<=(365)
- CK_SignInRule_Positive: [Points]>=(0) AND [Experience]>=(0)
- CK_SignInRule_CouponFlag: ([HasCoupon]=(1) AND [CouponTypeCode] IS NOT NULL) OR ([HasCoupon]=(0) AND [CouponTypeCode] IS NULL)

**索引**:
- UQ_SignInRule_SignInDay_Active (UNIQUE) on SignInDay
- IX_SignInRule_IsDeleted on IsDeleted

---

### 13. SystemSettings（系統設定）
**主鍵**: SettingId (int, IDENTITY)

**欄位**:
- SettingId (int, NOT NULL, IDENTITY) - PK
- SettingKey (nvarchar(200), NOT NULL) - UNIQUE
- SettingValue (nvarchar(MAX), NULL)
- Description (nvarchar(500), NULL)
- Category (nvarchar(100), NOT NULL, DEFAULT 'General')
- SettingType (nvarchar(50), NOT NULL, DEFAULT 'String') - 'String', 'Boolean', 'Number', 'JSON'
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
- CHK_SystemSettings_SettingType: [SettingType]='String' OR [SettingType]='Boolean' OR [SettingType]='Number' OR [SettingType]='JSON'

**索引**:
- UQ_SystemSettings_SettingKey (UNIQUE) on SettingKey

---

### 14. User_Wallet（會員錢包）
**主鍵**: User_Id (int)
**外鍵**:
- User_Id → Users.User_ID (NO_ACTION)

**欄位**:
- User_Id (int, NOT NULL) - PK & FK
- User_Point (int, NOT NULL, DEFAULT 0)
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**索引**:
- IX_User_Wallet_IsDeleted on IsDeleted

---

### 15. UserSignInStats（簽到統計記錄）
**主鍵**: LogID (int, IDENTITY)
**外鍵**:
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- LogID (int, NOT NULL, IDENTITY) - PK
- SignTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UserID (int, NOT NULL)
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

---

### 16. WalletHistory（錢包異動歷史）
**主鍵**: LogID (int, IDENTITY)
**外鍵**:
- UserID → Users.User_ID (NO_ACTION)

**欄位**:
- LogID (int, NOT NULL, IDENTITY) - PK
- UserID (int, NOT NULL)
- ChangeType (nvarchar(20), NOT NULL)
- PointsChanged (int, NOT NULL)
- ItemCode (nvarchar(50), NULL)
- Description (nvarchar(255), NULL)
- ChangeTime (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

**索引**:
- IX_WalletHistory_IsDeleted on IsDeleted
- IX_WalletHistory_type_time on (ChangeType, ChangeTime)
- IX_WalletHistory_user_time on (UserID, ChangeTime)

---

### 17. ManagerData（管理員基本資料）
**主鍵**: Manager_Id (int)

**欄位**:
- Manager_Id (int, NOT NULL) - PK
- Manager_Name (nvarchar(30), NULL)
- Manager_Account (varchar(30), NULL)
- Manager_Password (nvarchar(200), NULL)
- Administrator_registration_date (datetime2(7), NULL)
- Manager_Email (nvarchar(255), NOT NULL)
- Manager_EmailConfirmed (bit, NOT NULL, DEFAULT 0)
- Manager_AccessFailedCount (int, NOT NULL, DEFAULT 0)
- Manager_LockoutEnabled (bit, NOT NULL, DEFAULT 0)
- Manager_LockoutEnd (datetime2(7), NULL)

---

### 18. ManagerRole（管理員角色分配）
**主鍵**: 複合主鍵 (Manager_Id, ManagerRole_Id)
**外鍵**:
- Manager_Id → ManagerData.Manager_Id (NO_ACTION)
- ManagerRole_Id → ManagerRolePermission.ManagerRole_Id (NO_ACTION)

**欄位**:
- Manager_Id (int, NOT NULL) - PK & FK
- ManagerRole_Id (int, NOT NULL) - PK & FK

---

### 19. ManagerRolePermission（角色權限定義）
**主鍵**: ManagerRole_Id (int)

**欄位**:
- ManagerRole_Id (int, NOT NULL) - PK
- role_name (nvarchar(50), NOT NULL)
- AdministratorPrivilegesManagement (bit, NULL, DEFAULT NULL)
- UserStatusManagement (bit, NULL, DEFAULT NULL)
- ShoppingPermissionManagement (bit, NULL, DEFAULT NULL)
- MessagePermissionManagement (bit, NULL, DEFAULT NULL)
- Pet_Rights_Management (bit, NULL, DEFAULT NULL)
- customer_service (bit, NULL, DEFAULT NULL)

---

### 20. Users（使用者基本資料）
**主鍵**: User_ID (int, IDENTITY)

**欄位**:
- User_ID (int, NOT NULL, IDENTITY) - PK
- User_name (nvarchar(30), NOT NULL)
- User_Account (nvarchar(30), NOT NULL)
- User_Password (nvarchar(255), NOT NULL)
- User_EmailConfirmed (bit, NOT NULL, DEFAULT 0)
- User_PhoneNumberConfirmed (bit, NOT NULL, DEFAULT 0)
- User_TwoFactorEnabled (bit, NOT NULL, DEFAULT 0)
- User_AccessFailedCount (int, NOT NULL, DEFAULT 0)
- User_LockoutEnabled (bit, NOT NULL, DEFAULT 0)
- User_LockoutEnd (datetime2(7), NULL)
- Create_Account (datetime2(6), NOT NULL)

---

## 外鍵關係總結

### MiniGame Area 表格之間的關聯
- **Coupon** → CouponType (多對一)
- **Coupon** → Users (多對一)
- **EVoucher** → EVoucherType (多對一)
- **EVoucher** → Users (多對一)
- **EVoucherRedeemLog** → EVoucher (多對一, CASCADE DELETE)
- **EVoucherRedeemLog** → EVoucherToken (多對一)
- **EVoucherRedeemLog** → Users (多對一)
- **EVoucherToken** → EVoucher (多對一)
- **MiniGame** → Pet (多對一)
- **MiniGame** → Users (多對一)
- **Pet** → Users (多對一)
- **User_Wallet** → Users (一對一)
- **UserSignInStats** → Users (多對一)
- **WalletHistory** → Users (多對一)

### 管理員權限關聯
- **ManagerRole** → ManagerData (多對一)
- **ManagerRole** → ManagerRolePermission (多對一)

---

## 重要設計模式

### 1. 軟刪除（Soft Delete）模式
所有 MiniGame Area 主要表格都實作軟刪除：
- IsDeleted (bit, NOT NULL, DEFAULT 0)
- DeletedAt (datetime2(7), NULL)
- DeletedBy (int, NULL)
- DeleteReason (nvarchar(500), NULL)

### 2. 審計追蹤（Audit Trail）
部分表格有建立/更新追蹤：
- CreatedAt (datetime2(7), NOT NULL, DEFAULT sysutcdatetime())
- UpdatedAt (datetime2(7), NULL)
- UpdatedBy (int, NULL)

### 3. 時間戳記自動設定
多個欄位使用 DEFAULT sysutcdatetime()：
- AcquiredTime, UsedTime (Coupon, EVoucher)
- ScannedAt (EVoucherRedeemLog)
- SignTime, PointsGainedTime, ExpGainedTime, CouponGainedTime (UserSignInStats)
- ChangeTime (WalletHistory)
- StartTime, EndTime, ExpGainedTime, PointsGainedTime, CouponGainedTime (MiniGame)
- LevelUpTime, PointsGainedTime_LevelUp (Pet)

### 4. 資料完整性控制
- CHECK Constraints 用於：
  - 枚舉值驗證（DiscountType, Status, Rarity, SettingType）
  - 數值範圍驗證（Pet 屬性 0-100, 等級範圍, 點數範圍）
  - 日期範圍驗證（ValidFrom <= ValidTo）
  - 邏輯一致性（IsUsed 欄位與相關欄位的連動）
  - 格式驗證（ColorCode 必須以 # 開頭）

### 5. 唯一性約束
- CouponCode, EVoucherCode - 確保每個優惠券/禮券代碼唯一
- CouponType.Name - 確保優惠券類型名稱唯一
- SignInRule.SignInDay - 確保每天只有一個簽到規則
- SystemSettings.SettingKey - 確保設定鍵值唯一

---

## 索引策略

### 查詢效能優化索引
- **Coupon**: IX_Coupon_user_used (UserID, IsUsed, AcquiredTime) - 支援會員優惠券查詢
- **EVoucher**: IX_EVoucher_user_used (UserID, IsUsed, AcquiredTime) - 支援會員禮券查詢
- **WalletHistory**: IX_WalletHistory_user_time (UserID, ChangeTime) - 支援會員錢包歷史查詢
- **WalletHistory**: IX_WalletHistory_type_time (ChangeType, ChangeTime) - 支援異動類型查詢

### 軟刪除索引
多數表格有 IX_*_IsDeleted 索引，提升過濾已刪除記錄的效能

---

## 資料型別使用規範

### 字串型別
- **nvarchar**: 用於需要支援 Unicode 的欄位（中文等多語言支援）
  - 名稱、描述: nvarchar(50~600)
  - 長描述: nvarchar(MAX) 僅用於 SystemSettings.SettingValue
- **varchar**: 用於純 ASCII 字串
  - Token: varchar(64)
  - ColorCode: varchar(7) (#RRGGBB 格式)

### 數值型別
- **int**: 所有 ID、點數、經驗值、數量
- **decimal(18,2)**: 金額、折扣值 (CouponType, EVoucherType)
- **decimal(5,2)**: 倍率 (MiniGame.SpeedMultiplier)
- **bit**: 布林值 (IsUsed, IsDeleted, IsActive 等)

### 日期時間型別
- **datetime2(7)**: 所有時間戳記欄位（SQL Server 2008+ 建議型別，精確度更高）
- **datetime2(6)**: Users.Create_Account（較低精確度）

---

## 預設值策略

### 布林欄位
- IsDeleted: DEFAULT 0 (未刪除)
- IsUsed: DEFAULT 0 (未使用)
- IsActive: DEFAULT 1 (啟用)
- IsFree: DEFAULT 0 (非免費)
- IsRevoked: DEFAULT 0 (未撤銷)
- Aborted: DEFAULT 0 (未中止)

### 數值欄位
- User_Point: DEFAULT 0
- PointsCost: DEFAULT 2000 (PetSkinColorCostSettings)
- DisplayOrder: DEFAULT 0
- CurrentExperience: DEFAULT 0
- TotalPointsGained_LevelUp: DEFAULT 0

### 字串欄位
- Category: DEFAULT 'General' (SystemSettings)
- SettingType: DEFAULT 'String' (SystemSettings)
- Rarity: DEFAULT '普通' (PetSkinColorCostSettings)

### 時間戳記欄位
大量使用 DEFAULT sysutcdatetime() 提供 UTC 時間自動填入

---

**查詢方法**: 使用 sqlcmd 直接連線 SQL Server 查詢系統資料表
**參考來源**: sys.tables, sys.columns, sys.types, sys.indexes, sys.foreign_keys, sys.check_constraints, sys.default_constraints
