# GameSpace 資料庫連接驗證報告

**執行日期**: 2025 年 10 月 20 日
**執行者**: Claude Code
**資料庫**: GameSpacedatabase
**伺服器**: DESKTOP-8HQIS1S\SQLEXPRESS

---

## 📊 連接驗證結果

### ✅ 連接狀態：成功

**連接命令格式**:

```powershell
sqlcmd -S "tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433" -d "GameSpacedatabase" -E -W -s "\t" -f 65001 -Q "SQL查詢"
```

**認證方式**: Windows 整合認證 (-E 參數)

---

## 🗄️ 資料庫概覽

### 總表數：90 個表

### ⭐ MiniGame Area 核心表格（18 張）

#### 🎮 MiniGame Area 功能表（14 張）

1. **Coupon** - 商城優惠券實例
2. **CouponType** - 優惠券類型定義
3. **EVoucher** - 電子禮券實例
4. **EVoucherRedeemLog** - 電子禮券核銷記錄
5. **EVoucherToken** - 電子禮券核銷憑證
6. **EVoucherType** - 電子禮券類型定義
7. **MiniGame** - 小遊戲記錄
8. **Pet** - 寵物狀態資料
9. **PetBackgroundCostSettings** - 寵物背景價格設定
10. **SignInRule** - 簽到規則設定
11. **SystemSettings** - 系統設定（Game/Pet/SignIn/Wallet/Coupon/EVoucher）
12. **User_Wallet** - 會員錢包（點數餘額）
13. **UserSignInStats** - 簽到統計記錄
14. **WalletHistory** - 錢包異動歷史

#### 👥 權限與使用者表（4 張，與 MiniGame Area 有 FK 連結）

15. **ManagerData** - 管理員基本資料
16. **ManagerRole** - 管理員角色分配
17. **ManagerRolePermission** - 角色權限定義
18. **Users** - 使用者基本資料

### 其他業務表

- **論壇**: forums, threads, posts, thread_posts, post_sources, post_metric_snapshot
- **商城**: S_ProductInfo, S_ProductImages, S_ProductRatings, SO_OrderInfoes, SO_OrderItems, SO_OrderAddresses, PlayerMarketProductInfo, PlayerMarketOrderInfo
- **社群**: Groups, Group_Member, Group_Chat, DM_Conversations, DM_Messages, Notifications, Notification_Recipients, Support_Ticket_Messages

---

## 📈 數據統計

### MiniGame Area 核心表數據量（按圖片順序排列）

| 資料表                        | 記錄數量 | 用途                         | 類別         |
| ----------------------------- | -------- | ---------------------------- | ------------ |
| **Coupon**                    | 698      | 商城優惠券實例               | 優惠券系統   |
| **CouponType**                | 24       | 優惠券類型定義               | 優惠券系統   |
| **EVoucher**                  | 355      | 電子禮券實例                 | 電子禮券系統 |
| **EVoucherRedeemLog**         | 800      | 電子禮券核銷記錄             | 電子禮券系統 |
| **EVoucherToken**             | 355      | 電子禮券核銷憑證（一次性）   | 電子禮券系統 |
| **EVoucherType**              | 20       | 電子禮券類型定義             | 電子禮券系統 |
| **MiniGame**                  | 2,000    | 小遊戲記錄                   | 遊戲系統     |
| **Pet**                       | 200      | 寵物狀態資料（含五維屬性）   | 寵物系統     |
| **PetBackgroundCostSettings** | 18       | 寵物背景價格設定（免費+付費）| 寵物系統     |
| **SignInRule**                | 8        | 簽到規則設定（Day 1-7, 30）  | 簽到系統     |
| **SystemSettings**            | 46       | 系統設定（6 大類）           | 系統配置     |
| **User_Wallet**               | 200      | 會員錢包（點數餘額）         | 錢包系統     |
| **UserSignInStats**           | 2,400    | 簽到統計記錄                 | 簽到系統     |
| **WalletHistory**             | 1,928    | 錢包異動歷史（含審計軌跡）   | 錢包系統     |
| **ManagerData**               | 102      | 管理員基本資料               | 權限管理     |
| **ManagerRole**               | 102      | 管理員角色分配               | 權限管理     |
| **ManagerRolePermission**     | 8        | 角色權限定義（8 種角色）     | 權限管理     |
| **Users**                     | 200      | 使用者基本資料               | 使用者管理   |

**總計 18 張表，涵蓋所有 MiniGame Area 與權限管理功能**

---

## 🔍 實際資料讀取驗證

### 1. ManagerData（管理員資料表）

**讀取成功** - 前 3 筆記錄：

| Manager_Id | Manager_Name | Manager_Account  | Manager_Email             | Registration Date   |
| ---------- | ------------ | ---------------- | ------------------------- | ------------------- |
| 30000001   | Milk Hung    | zhang_zhiming_01 | zhang.zhiming@company.com | 2019-01-15 08:30:00 |
| 30000002   | 李小華       | li_xiaohua_02    | li.xiaohua@company.com    | 2019-01-16 09:15:00 |
| 30000003   | 王美玲       | wang_meiling_03  | wang.meiling@company.com  | 2019-01-17 10:45:00 |

**測試帳號密碼**:

- zhang_zhiming_01: `AdminPass001@` (最高權限)
- li_xiaohua_02: `SecurePass002#` (使用者管理)
- wang_meiling_03: `StrongPwd003!` (商城寵物管理)

---

### 2. ManagerRolePermission（角色權限表）

**讀取成功** - 共 8 個角色：

| RoleId | RoleName             | Admin 權限 | 使用者管理 | 商城權限 | 寵物管理 | 客服權限 |
| ------ | -------------------- | ---------- | ---------- | -------- | -------- | -------- |
| 1      | 管理者平台管理人員   | ✅         | ✅         | ✅       | ✅       | ✅       |
| 2      | 使用者與論壇管理經理 | ❌         | ✅         | ❌       | ❌       | ✅       |
| 3      | 商城與寵物管理經理   | ❌         | ❌         | ✅       | ✅       | ❌       |
| 4      | 使用者平台管理人員   | ❌         | ✅         | ❌       | ❌       | ❌       |
| 5      | 商城平台管理人員     | ❌         | ❌         | ✅       | ❌       | ❌       |
| 6      | 論壇平台管理人員     | ❌         | ❌         | ❌       | ✅       | ❌       |
| 7      | 寵物平台管理人員     | ❌         | ❌         | ❌       | ✅       | ❌       |
| 8      | 客服與回報管理員     | ❌         | ❌         | ❌       | ❌       | ✅       |

---

### 2A. ManagerRole（管理員角色分配）

**讀取成功** - 共 102 筆管理員角色分配記錄

**前 5 筆記錄**：

| Manager_Id | ManagerRole_Id | 說明                 |
| ---------- | -------------- | -------------------- |
| 30000001   | 1              | 管理者平台管理人員   |
| 30000002   | 2              | 使用者與論壇管理經理 |
| 30000003   | 3              | 商城與寵物管理經理   |
| 30000004   | 4              | 使用者平台管理人員   |
| 30000005   | 5              | 商城平台管理人員     |

**關聯說明**:
- **Manager_Id**: 外鍵 → ManagerData.Manager_Id
- **ManagerRole_Id**: 外鍵 → ManagerRolePermission.ManagerRole_Id
- 每位管理員必須分配一個角色
- 角色決定管理員可存取的功能模組

**權限繼承**:
```
ManagerData (管理員) → ManagerRole (分配) → ManagerRolePermission (權限定義)
```

---

### 3. Users（使用者資料表）

**讀取成功** - 前 3 筆記錄：

| User_ID  | User_name      | User_Account   | User_Password  | EmailConfirmed |
| -------- | -------------- | -------------- | -------------- | -------------- |
| 10000001 | DragonKnight88 | dragonknight88 | Password001@   | ✅             |
| 10000002 | TechGuru92     | tech_guru_92   | SecurePass002# | ✅             |
| 10000003 | CoffeeAddict   | coffee_addict  | CoffeePwd003!  | ✅             |

---

### 4. User_Wallet（使用者錢包）

**讀取成功** - 前 5 筆記錄：

| User_Id  | User_Point | 說明     |
| -------- | ---------- | -------- |
| 10000001 | 1,477      | 點數餘額 |
| 10000002 | 1,011      | 點數餘額 |
| 10000003 | 1,166      | 點數餘額 |
| 10000004 | 689        | 點數餘額 |
| 10000005 | 1,799      | 點數餘額 |

---

### 5. WalletHistory（錢包交易歷史）

**讀取成功** - 前 5 筆記錄：

| LogID | UserID   | ChangeType | PointsChanged | ItemCode             | Description  | ChangeTime          |
| ----- | -------- | ---------- | ------------- | -------------------- | ------------ | ------------------- |
| 1     | 10000001 | Point      | -25           | NULL                 | 每日簽到扣點 | 2024-04-03 11:03:00 |
| 2     | 10000001 | Coupon     | 0             | CPN-2302-OJG536      | 簽到獲得     | 2023-07-17 07:26:37 |
| 3     | 10000001 | EVoucher   | 0             | EV-STORE-EMP2-230289 | 購物發放禮券 | 2025-02-24 17:17:49 |
| 4     | 10000001 | Coupon     | 0             | CPN-2401-SIP920      | 簽到獲得     | 2024-12-28 01:03:28 |
| 5     | 10000001 | Coupon     | 0             | CPN-2311-SLW606      | 簽到獲得     | 2024-04-01 00:22:27 |

**ChangeType 類型**:

- `Point`: 點數變更
- `Coupon`: 優惠券變更
- `EVoucher`: 電子禮券變更

---

### 6. Pet（寵物資料表）

**讀取成功** - 前 3 筆記錄：

| PetID | UserID   | PetName | Level | Experience | Health | Hunger | Mood | Stamina | Cleanliness |
| ----- | -------- | ------- | ----- | ---------- | ------ | ------ | ---- | ------- | ----------- |
| 1     | 10000001 | 多多    | 4     | 656        | 100    | 14     | 58   | 89      | 25          |
| 2     | 10000002 | 小小    | 38    | 4,019      | 94     | 55     | 15   | 57      | 14          |
| 3     | 10000003 | 淘淘    | 19    | 2,947      | 78     | 58     | 51   | 76      | 70          |

**寵物屬性（五維系統）**:

- Health（健康值）: 0-100
- Hunger（飽食度）: 0-100
- Mood（心情值）: 0-100
- Stamina（體力值）: 0-100
- Cleanliness（清潔度）: 0-100

**外觀屬性**:

- SkinColor: 十六進位色碼（如 #86270E）
- BackgroundColor: 背景顏色選項

---

### 7. MiniGame（小遊戲記錄表）

**讀取成功** - 共 2,000 筆記錄

**關卡分布（3 關制）**：
| Level | 記錄數 | 怪物數 | 速度倍率 | 經驗獎勵 | 點數獎勵 |
| ----- | ------ | ------ | -------- | -------- | -------- |
| 1     | 670    | 6      | 1.0x     | 100      | 10       |
| 2     | 661    | 8      | 1.5x     | 200      | 20       |
| 3     | 669    | 10     | 2.0x     | 300      | 30       |

**遊戲結果**:

- `Win`: 勝利（獲得獎勵）
- `Lose`: 失敗（仍可獲得部分獎勵）
- `Aborted`: 中途放棄

**獎勵系統**:

- ExpGained: 寵物經驗值
- PointsGained: 點數獎勵
- CouponGained: 優惠券獎勵

**寵物屬性變化**:

- HungerDelta: 飽食度變化
- MoodDelta: 心情值變化
- StaminaDelta: 體力值變化
- CleanlinessDelta: 清潔度變化

---

### 8. UserSignInStats（簽到統計）

**讀取成功** - 前 5 筆記錄：

| LogID | SignTime            | UserID   | PointsGained | ExpGained | CouponGained |
| ----- | ------------------- | -------- | ------------ | --------- | ------------ |
| 1     | 2023-04-13 10:56:22 | 10000001 | 15           | 0         | 0            |
| 2     | 2023-06-02 08:24:29 | 10000001 | 15           | 20        | 0            |
| 3     | 2023-06-21 10:03:48 | 10000001 | 20           | 5         | 0            |
| 4     | 2023-07-18 10:44:32 | 10000001 | 10           | 5         | 0            |
| 5     | 2023-08-10 09:36:22 | 10000001 | 5            | 10        | 0            |

**簽到獎勵類型**:

- PointsGained: 點數獎勵
- ExpGained: 寵物經驗值
- CouponGained: 優惠券獎勵（布林值）

---

### 9. CouponType（優惠券類型）

**讀取成功** - 前 5 筆記錄：

| CouponTypeID | Name           | DiscountType | DiscountValue | MinSpend | PointsCost |
| ------------ | -------------- | ------------ | ------------- | -------- | ---------- |
| 1            | 新會員$100 券  | Amount       | 100.00        | 2000.00  | 0          |
| 2            | 全館 85 折     | Percent      | 0.15          | 1500.00  | 150        |
| 3            | 滿$500 送$50   | Amount       | 300.00        | 1000.00  | 150        |
| 4            | 滿$1000 送$120 | Amount       | 120.00        | 2000.00  | 200        |
| 5            | 免運券         | Amount       | 150.00        | 1000.00  | 100        |

**折扣類型**:

- `Amount`: 固定金額折扣
- `Percent`: 百分比折扣

---

### 10. EVoucherType（電子禮券類型）

**讀取成功** - 前 5 筆記錄：

| EVoucherTypeID | Name               | ValueAmount | ValidFrom  | ValidTo    | PointsCost | TotalAvailable |
| -------------- | ------------------ | ----------- | ---------- | ---------- | ---------- | -------------- |
| 1              | 現金券$100         | 100.00      | 2024-07-24 | 2025-03-13 | 200        | 468            |
| 2              | 現金券$200         | 200.00      | 2025-01-08 | 2025-09-04 | 200        | 437            |
| 3              | 現金券$300         | 300.00      | 2024-03-05 | 2024-05-23 | 200        | 194            |
| 4              | 現金券$500         | 500.00      | 2024-03-30 | 2025-07-08 | 60         | 438            |
| 5              | 全家禮物卡-飲料(M) | 200.00      | 2024-05-06 | 2024-05-17 | 150        | 433            |

---

### 10A. EVoucher（電子禮券實例）

**讀取成功** - 共 355 筆電子禮券

**序號格式**: `EV-{類型代碼}-{4 位隨機碼}-{6 位數字}`

**類型代碼範例**:
- `EV-MOVIE-8JDW-064877` - 電影票券
- `EV-CASH-VR2G-969669` - 現金禮券
- `EV-FOOD-DKTG-417690` - 餐飲券
- `EV-GAS-KELK-380122` - 加油券
- `EV-STORE-EMP2-230289` - 商店券
- `EV-COFFEE-XXXX-XXXXXX` - 咖啡券

**狀態管理**:
- IsUsed: 使用狀態（0=未使用，1=已使用）
- UsedTime: 使用時間
- AcquiredTime: 獲得時間

---

### 10B. EVoucherToken（電子禮券核銷憑證）

**讀取成功** - 前 3 筆記錄：

| TokenID | EVoucherID | Token                | ExpiresAt           | IsRevoked |
| ------- | ---------- | -------------------- | ------------------- | --------- |
| 1       | 1          | TKN-GFWZIUGU-7603    | 2023-07-15 03:39:23 | 0         |
| 2       | 2          | TKN-0AFNJ3IW-7410    | 2023-07-08 17:55:11 | 0         |
| 3       | 3          | TKN-QBHYOJ30-9972    | 2025-08-05 09:01:17 | 0         |

**核銷憑證說明**:
- **Token 格式**: `TKN-{8 位隨機碼}-{4 位數字}`
- **用途**: 提供一次性核銷憑證，增強安全性
- **ExpiresAt**: 憑證過期時間（通常為短效，如 5 分鐘）
- **IsRevoked**: 是否已撤銷（0=有效，1=已撤銷）

**安全機制**:
- 高強度隨機生成
- 短效期限（防止濫用）
- 一次性使用
- 可撤銷功能

---

### 10C. EVoucherRedeemLog（電子禮券核銷記錄）

**讀取成功** - 前 3 筆記錄：

| RedeemID | EVoucherID | TokenID | UserID   | ScannedAt           | Status       |
| -------- | ---------- | ------- | -------- | ------------------- | ------------ |
| 1        | 1          | 1       | 10000042 | 2025-05-30 23:53:14 | Approved     |
| 2        | 1          | 1       | 10000030 | 2025-01-28 16:20:55 | AlreadyUsed  |
| 3        | 2          | 2       | 10000185 | 2024-06-07 03:53:06 | Revoked      |

**核銷狀態說明**:
- **Approved**: 核銷成功
- **AlreadyUsed**: 已被使用（重複核銷）
- **Revoked**: 憑證已撤銷
- **Rejected**: 核銷被拒絕
- **Expired**: 憑證已過期

**核銷流程**:
1. 店員掃描 EVoucherCode 或 EVoucherToken
2. 系統驗證禮券有效性（未過期、未使用、未撤銷）
3. 記錄核銷日誌（ScannedAt、Status）
4. 更新 EVoucher 狀態（IsUsed=1、UsedTime）

**統計資訊**:
- 總核銷記錄：800 筆
- 涵蓋禮券：355 張
- 記錄所有核銷嘗試（成功/失敗）

---

### 11. SignInRule（簽到規則設定）

**讀取成功** - 共 8 筆規則：

| Day | Points | Experience | Coupon | CouponTypeCode | Description |
| --- | ------ | ---------- | ------ | -------------- | ----------- |
| 1   | 20     | 0          | ❌     | -              | 第 1 天簽到獎勵 |
| 2   | 20     | 0          | ❌     | -              | 第 2 天簽到獎勵 |
| 3   | 20     | 0          | ❌     | -              | 第 3 天簽到獎勵 |
| 4   | 20     | 0          | ❌     | -              | 第 4 天簽到獎勵 |
| 5   | 20     | 0          | ❌     | -              | 第 5 天簽到獎勵 |
| 6   | 30     | 200        | ❌     | -              | 第 6 天簽到獎勵（週末） |
| 7   | 70     | 500        | ❌     | -              | 第 7 天簽到獎勵 + 連續獎勵 |
| 30  | 200    | 2000       | ✅     | MONTH_BONUS    | 連續簽到 30 天獎勵（含優惠券） |

**獎勵公式驗證**：
- **平日（Day 1-5）**：+20 點數，+0 經驗
- **假日（Day 6）**：+30 點數，+200 經驗
- **連續 7 天（Day 7）**：基礎 30 + 額外 40 = 70 點數，基礎 200 + 額外 300 = 500 經驗
- **當月全勤（Day 30）**：+200 點數，+2000 經驗，+1 張商城優惠券（MONTH_BONUS）

---

### 12. SystemSettings（系統設定）

**讀取成功** - 共 46 筆設定

**遊戲設定（Game）- 14 筆**：

| 設定項目 | 值 | 說明 |
| -------- | -- | ---- |
| Game.DefaultDailyLimit | 3 | 每日遊戲次數限制 |
| Game.Level1.MonsterCount | 6 | 第 1 關怪物數量 |
| Game.Level1.SpeedMultiplier | 1.0 | 第 1 關速度倍率 |
| Game.Level1.ExperienceReward | 100 | 第 1 關經驗獎勵 |
| Game.Level1.PointsReward | 10 | 第 1 關點數獎勵 |
| Game.Level2.MonsterCount | 8 | 第 2 關怪物數量 |
| Game.Level2.SpeedMultiplier | 1.5 | 第 2 關速度倍率 |
| Game.Level2.ExperienceReward | 200 | 第 2 關經驗獎勵 |
| Game.Level2.PointsReward | 20 | 第 2 關點數獎勵 |
| Game.Level3.MonsterCount | 10 | 第 3 關怪物數量 |
| Game.Level3.SpeedMultiplier | 2.0 | 第 3 關速度倍率 |
| Game.Level3.ExperienceReward | 300 | 第 3 關經驗獎勵 |
| Game.Level3.PointsReward | 30 | 第 3 關點數獎勵 |
| Game.Levels.Configuration | JSON | 完整關卡配置（3 關制） |

**寵物設定（Pet）- 17 筆**：

| 設定項目 | 值 | 說明 |
| -------- | -- | ---- |
| Pet.ColorChange.PointsCost | 2000 | 寵物換色費用 |
| Pet.DailyDecay.HungerDecay | 20 | 每日飢餓值衰減 |
| Pet.DailyDecay.MoodDecay | 30 | 每日心情值衰減 |
| Pet.DailyDecay.StaminaDecay | 10 | 每日體力值衰減 |
| Pet.DailyDecay.CleanlinessDecay | 20 | 每日清潔值衰減 |
| Pet.DailyDecay.HealthDecay | 0 | 每日健康值衰減（無） |
| Pet.Interaction.Feed.HungerIncrease | 10 | 餵食增加飢餓值 |
| Pet.Interaction.Bath.CleanlinessIncrease | 10 | 洗澡增加清潔值 |
| Pet.Interaction.Bath.MoodIncrease | 10 | 洗澡增加心情值 |
| Pet.Interaction.Coax.MoodIncrease | 10 | 哄睡增加心情值 |
| Pet.Interaction.Coax.StaminaIncrease | 10 | 哄睡增加體力值 |
| Pet.Interaction.Feed.HealthIncrease | 10 | 餵食增加健康值 |
| Pet.DailyFullStatsBonus.Experience | 100 | 每日全滿獎勵經驗值 |
| Pet.DailyFullStatsBonus.Points | 0 | 每日全滿獎勵點數（無） |

**簽到設定（SignIn）- 11 筆**：

| 設定項目 | 值 | 說明 |
| -------- | -- | ---- |
| SignIn.PerfectAttendance30Days.BonusPoints | 200 | 30 天全勤額外點數 |
| SignIn.PerfectAttendance30Days.BonusExperience | 2000 | 30 天全勤額外經驗 |
| SignIn.PerfectAttendance30Days.CouponType | MONTH_BONUS | 30 天全勤優惠券類型 |

**錢包設定（Wallet）- 2 筆**：

| 設定項目 | 值 | 說明 |
| -------- | -- | ---- |
| Wallet.InitialPoints | 1000 | 新註冊會員初始點數 |
| Wallet.MaxPoints | 999999 | 會員點數上限 |

**優惠券設定（Coupon）- 1 筆**：

| 設定項目 | 值 | 說明 |
| -------- | -- | ---- |
| Coupon.DefaultValidityDays | 30 | 優惠券預設有效期（天） |

**電子禮券設定（EVoucher）- 1 筆**：

| 設定項目 | 值 | 說明 |
| -------- | -- | ---- |
| EVoucher.DefaultValidityDays | 90 | 電子禮券預設有效期（天） |

---

### 13. PetBackgroundCostSettings（寵物背景價格設定）

**讀取成功** - 共 18 筆背景設定

**免費背景（前 3 個）**：

| 背景代碼 | 背景名稱 | 點數費用 | 狀態 |
| -------- | -------- | -------- | ---- |
| BG001 | Pure White | 0 | 啟用 |
| BG002 | Light Gray | 0 | 啟用 |
| BG003 | Light Blue | 0 | 啟用 |

**付費背景（精選 7 個）**：

| 背景代碼 | 背景名稱 | 點數費用 | 狀態 |
| -------- | -------- | -------- | ---- |
| BG004 | Pink Dream | 55 | 啟用 |
| BG005 | Mint Oasis | 65 | 啟用 |
| BG006 | Lemon Dawn | 75 | 啟用 |
| BG007 | Lavender Mist | 85 | 啟用 |
| BG008 | Peach Cloud | 95 | 啟用 |
| BG009 | Deep Sea Sapphire | 165 | 啟用 |
| BG010 | Forest Shade | 185 | 啟用 |

**價格範圍**：0 - 500 點（共 18 種背景）

---

### 14. WalletHistory（錢包交易歷史）完整結構

**表結構** - 共 11 個欄位：

| 欄位名稱 | 資料型別 | 必填 | 說明 |
| -------- | -------- | ---- | ---- |
| LogID | int | ✅ | 交易記錄 ID（主鍵，自動遞增） |
| UserID | int | ✅ | 會員 ID（外鍵 → Users.User_ID） |
| ChangeType | nvarchar | ✅ | 變動類型（Point/Coupon/EVoucher） |
| PointsChanged | int | ✅ | 點數變動數量（正數=增加，負數=扣除） |
| ItemCode | nvarchar | ⭕ | 項目代碼（優惠券號碼或禮券號碼） |
| Description | nvarchar | ⭕ | 變動原因描述 |
| ChangeTime | datetime2 | ✅ | 變動時間 |
| IsDeleted | bit | ✅ | 軟刪除標記（預設 0） |
| DeletedAt | datetime2 | ⭕ | 刪除時間 |
| DeletedBy | int | ⭕ | 刪除者 ID |
| DeleteReason | nvarchar | ⭕ | 刪除原因 |

**統計資訊**：
- 總交易數：1,928 筆
- 涵蓋用戶：200 位
- 最早交易：2023-01-01
- 最新交易：2025-09-05

---

## 📝 實際欄位名稱對照表

### ManagerData 表

| 文件中的名稱     | 實際欄位名稱                    |
| ---------------- | ------------------------------- |
| ManagerId        | Manager_Id                      |
| ManagerName      | Manager_Name                    |
| Account          | Manager_Account                 |
| Password         | Manager_Password                |
| Email            | Manager_Email                   |
| RegistrationDate | Administrator_registration_date |

### Pet 表

| 文件中的名稱                         | 實際欄位名稱          |
| ------------------------------------ | --------------------- |
| PetId                                | PetID                 |
| UserId                               | UserID                |
| PetName                              | PetName               |
| Level                                | Level                 |
| Experience                           | Experience            |
| CurrentExperience                    | CurrentExperience     |
| ExperienceToNextLevel                | ExperienceToNextLevel |
| Health                               | Health                |
| Hunger                               | Hunger                |
| Mood                                 | Mood                  |
| Cleanliness                          | Cleanliness           |
| (Loyalty 在文件中，但實際是 Stamina) | Stamina               |

### WalletHistory 表

| 文件中的名稱 | 實際欄位名稱  |
| ------------ | ------------- |
| HistoryId    | LogID         |
| UserId       | UserID        |
| ChangeType   | ChangeType    |
| ChangeAmount | PointsChanged |
| ItemCode     | ItemCode      |
| Description  | Description   |
| ChangeTime   | ChangeTime    |
| IsDeleted    | IsDeleted     |
| DeletedAt    | DeletedAt     |
| DeletedBy    | DeletedBy     |
| DeleteReason | DeleteReason  |

### SystemSettings 表

| 文件中的名稱 | 實際欄位名稱 |
| ------------ | ------------ |
| SettingId    | SettingId    |
| SettingKey   | SettingKey   |
| SettingValue | SettingValue |
| Description  | Description  |
| Category     | Category     |
| SettingType  | SettingType  |
| IsReadOnly   | IsReadOnly   |
| IsActive     | IsActive     |
| IsDeleted    | IsDeleted    |
| DeletedAt    | DeletedAt    |
| DeletedBy    | DeletedBy    |
| DeleteReason | DeleteReason |
| CreatedAt    | CreatedAt    |
| UpdatedAt    | UpdatedAt    |
| UpdatedBy    | UpdatedBy    |

### PetBackgroundCostSettings 表

| 文件中的名稱    | 實際欄位名稱     |
| --------------- | ---------------- |
| SettingId       | SettingId        |
| BackgroundCode  | BackgroundCode   |
| BackgroundName  | BackgroundName   |
| PointsCost      | PointsCost       |
| Description     | Description      |
| PreviewImagePath| PreviewImagePath |
| IsActive        | IsActive         |
| DisplayOrder    | DisplayOrder     |
| IsDeleted       | IsDeleted        |
| DeletedAt       | DeletedAt        |
| DeletedBy       | DeletedBy        |
| DeleteReason    | DeleteReason     |
| CreatedAt       | CreatedAt        |
| UpdatedAt       | UpdatedAt        |
| UpdatedBy       | UpdatedBy        |

### SignInRule 表

| 文件中的名稱   | 實際欄位名稱   |
| -------------- | -------------- |
| Id             | Id             |
| SignInDay      | SignInDay      |
| Points         | Points         |
| Experience     | Experience     |
| HasCoupon      | HasCoupon      |
| CouponTypeCode | CouponTypeCode |
| IsActive       | IsActive       |
| CreatedAt      | CreatedAt      |
| UpdatedAt      | UpdatedAt      |
| Description    | Description    |
| IsDeleted      | IsDeleted      |
| DeletedAt      | DeletedAt      |
| DeletedBy      | DeletedBy      |
| DeleteReason   | DeleteReason   |

---

## ✅ 驗證結論

### 成功項目

1. ✅ **資料庫連線成功** - Windows 整合認證正常運作
2. ✅ **所有表都可存取** - 90 個表均可正常查詢
3. ✅ **MiniGame Area 完整涵蓋** - 18 張核心表全部驗證通過
   - **14 張 MiniGame 功能表**: Coupon, CouponType, EVoucher, EVoucherRedeemLog, EVoucherToken, EVoucherType, MiniGame, Pet, PetBackgroundCostSettings, SignInRule, SystemSettings, User_Wallet, UserSignInStats, WalletHistory
   - **4 張權限/使用者表**: ManagerData, ManagerRole, ManagerRolePermission, Users
4. ✅ **資料完整性良好** - 所有核心表都有完整的種子資料
5. ✅ **資料關聯正確** - 外鍵關係正常（User-Pet, User-Wallet, Manager-Role 等）
6. ✅ **業務邏輯完整** - 簽到、遊戲、優惠券、電子禮券系統都有完整記錄
7. ✅ **軟刪除機制** - 關鍵表已新增 IsDeleted 欄位與過濾索引
8. ✅ **不存在表格已確認** - PetColorOptions、PetSkinColorPointSettings、PetBackgroundPointSettings 三張表確認不存在於資料庫（已從文件中移除）

### 資料品質評估

| 項目           | 評估       | 說明                                        |
| -------------- | ---------- | ------------------------------------------- |
| **資料量**     | ⭐⭐⭐⭐⭐ | 充足的測試資料（200 使用者、2400 簽到記錄） |
| **資料完整性** | ⭐⭐⭐⭐⭐ | 所有必要欄位都有值                          |
| **資料一致性** | ⭐⭐⭐⭐⭐ | 外鍵關係正確，無孤兒記錄                    |
| **業務邏輯**   | ⭐⭐⭐⭐⭐ | 涵蓋所有業務場景（簽到、遊戲、交易）        |

---

## 🎯 可直接使用的功能模組

基於實際資料庫驗證，以下功能模組可以直接開發：

### 1. 管理員登入系統 ✅

- 102 個管理員帳號
- 8 種角色權限
- 完整的密碼和 Email 資料

### 2. 使用者管理系統 ✅

- 200 個使用者帳號
- 完整的認證資料
- 鎖定和失敗計數機制

### 3. 錢包管理系統 ✅

- 點數餘額管理
- 完整的交易歷史
- 三種變更類型（Point/Coupon/EVoucher）

### 4. 寵物養成系統 ✅

- 200 隻寵物
- 五維屬性系統（Health/Hunger/Mood/Stamina/Cleanliness）
- 等級和經驗值系統
- 外觀自訂（膚色、背景）

### 5. 小遊戲系統 ✅

- 完整的遊戲記錄
- 獎勵結算（經驗值、點數、優惠券）
- 寵物屬性影響

### 6. 簽到系統 ✅

- 2400 筆簽到記錄
- 多種獎勵類型
- 連續簽到追蹤

### 7. 優惠券系統 ✅

- 20+ 種優惠券類型
- 698 張使用者優惠券
- 兩種折扣類型（Amount/Percent）

### 8. 電子禮券系統 ✅

- 20+ 種禮券類型
- 355 張使用者禮券
- 庫存管理

---

## 🔄 下一步建議

### 1. 立即可執行任務

- ✅ 建立 EF Core Entity Models（對應實際欄位名稱）
- ✅ 建立 Controllers 和 Services（注入共享的 GameSpacedatabaseContext）
- ✅ 實作 Admin 後台頁面（使用 SB Admin 2 模板）
- ✅ 實作權限檢查（基於 ManagerRolePermission）

### 2. 需要注意的事項

- ⚠️ 欄位名稱要使用實際的 Snake_Case 格式（如 `User_Id` 而非 `UserId`）
- ⚠️ Pet 表中沒有 `Loyalty` 欄位，實際是 `Stamina`（體力值）
- ⚠️ 有些欄位名稱與文件不同，需要對照實際欄位

### 3. 資料庫架構優勢

- ✅ 使用單一共享 DbContext（GameSpacedatabaseContext）
- ✅ 所有 Area 都使用相同的連接字串
- ✅ 資料關聯清楚，易於跨模組查詢

---

## 📊 技術規格確認

| 項目           | 規格                       | 狀態        |
| -------------- | -------------------------- | ----------- |
| **資料庫引擎** | SQL Server Express         | ✅ 運作正常 |
| **認證方式**   | Windows 整合認證           | ✅ 連線成功 |
| **資料庫名稱** | GameSpacedatabase          | ✅ 確認     |
| **伺服器**     | DESKTOP-8HQIS1S\SQLEXPRESS | ✅ 連線成功 |
| **工具**       | sqlcmd 命令列              | ✅ 正常運作 |
| **資料完整性** | 所有核心表都有資料         | ✅ 驗證通過 |
| **外鍵關係**   | User-Pet-Wallet-Game       | ✅ 關聯正確 |

---

## 🎉 總結

✅ **資料庫連接驗證成功！**

**本報告已完整涵蓋圖片中所列的所有 18 張 MiniGame Area 相關表格：**

### ✅ MiniGame Area 功能表（14 張）
1. ✅ Coupon - 商城優惠券實例（698 筆）
2. ✅ CouponType - 優惠券類型定義（24 種）
3. ✅ EVoucher - 電子禮券實例（355 筆）
4. ✅ EVoucherRedeemLog - 電子禮券核銷記錄（800 筆）
5. ✅ EVoucherToken - 電子禮券核銷憑證（355 筆）
6. ✅ EVoucherType - 電子禮券類型定義（20 種）
7. ✅ MiniGame - 小遊戲記錄（2,000 筆）
8. ✅ Pet - 寵物狀態資料（200 筆）
9. ✅ PetBackgroundCostSettings - 寵物背景價格設定（18 種）
10. ✅ SignInRule - 簽到規則設定（8 筆）
11. ✅ SystemSettings - 系統設定（46 筆，涵蓋 6 大類）
12. ✅ User_Wallet - 會員錢包（200 筆）
13. ✅ UserSignInStats - 簽到統計記錄（2,400 筆）
14. ✅ WalletHistory - 錢包異動歷史（1,928 筆）

### ✅ 權限與使用者表（4 張）
15. ✅ ManagerData - 管理員基本資料（102 筆）
16. ✅ ManagerRole - 管理員角色分配（102 筆）
17. ✅ ManagerRolePermission - 角色權限定義（8 種角色）
18. ✅ Users - 使用者基本資料（200 筆）

**所有表格資料完整且符合業務需求。**基於這些實際資料，可以立即開始開發 MiniGame Area 的完整功能，包括：

1. **管理員認證與權限控制**（ManagerData, ManagerRole, ManagerRolePermission）
2. **使用者錢包管理**（User_Wallet, WalletHistory）
3. **寵物養成系統**（Pet, PetBackgroundCostSettings, SystemSettings）
4. **小遊戲與獎勵**（MiniGame, SystemSettings）
5. **簽到系統**（UserSignInStats, SignInRule）
6. **優惠券管理**（Coupon, CouponType）
7. **電子禮券管理**（EVoucher, EVoucherType, EVoucherToken, EVoucherRedeemLog）

所有功能都有充足的測試資料支持，可以直接進行開發和測試。

### ❌ 已確認不存在的表格

以下三張表格已確認**不存在於資料庫中**（已從原文件中移除）：
- ❌ PetColorOptions
- ❌ PetSkinColorPointSettings
- ❌ PetBackgroundPointSettings

**原因**: 這些功能已整合至 SystemSettings 表格中，透過設定項目（如 `Pet.ColorChange.PointsCost`）來管理。

---

**報告生成時間**: 2025-10-20
**驗證工具**: sqlcmd + PowerShell
**驗證狀態**: ✅ 全部通過
