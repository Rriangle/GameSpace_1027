---
**更新記錄**:
- 2025-10-27: 驗證日期更新，確認表格清單與實際資料庫一致

---

# MiniGame Area 相關表格

**最後驗證日期**: 2025-10-27
**資料庫伺服器**: DESKTOP-8HQIS1S\SQLEXPRESS
**總表數**: 20 張（16 張主要表格 + 4 張關聯表格）

## MiniGame Area 主要表格 (16 張)

- `dbo.Coupon` - 優惠券實例
- `dbo.CouponType` - 優惠券類型定義
- `dbo.EVoucher` - 電子禮券實例
- `dbo.EVoucherRedeemLog` - 電子禮券核銷記錄
- `dbo.EVoucherToken` - 電子禮券核銷憑證
- `dbo.EVoucherType` - 電子禮券類型定義
- `dbo.MiniGame` - 小遊戲記錄
- `dbo.Pet` - 寵物系統資料
- `dbo.PetBackgroundCostSettings` - 寵物背景價格設定 ✨ **(2025-10-20 新增)**
- `dbo.PetLevelRewardSettings` - 寵物升級獎勵規則 ✨ **(2025-10-20 新增)**
- `dbo.PetSkinColorCostSettings` - 寵物膚色價格設定 ✨ **(2025-10-20 新增)**
- `dbo.SignInRule` - 簽到規則設定
- `dbo.SystemSettings` - 系統設定
- `dbo.User_Wallet` - 會員錢包
- `dbo.UserSignInStats` - 簽到統計記錄
- `dbo.WalletHistory` - 錢包異動歷史

## 使用者 / 權限 相關表格 (4 張)

這些表格與 MiniGame Area 有 FK 關聯，用於管理員權限和使用者資料：

- `dbo.ManagerData` - 管理員基本資料
- `dbo.ManagerRole` - 管理員角色分配
- `dbo.ManagerRolePermission` - 角色權限定義
- `dbo.Users` - 使用者基本資料

---

**完整結構文件**: 請參閱 `MiniGame_Area_資料庫完整結構文件_2025-10-21.md`
