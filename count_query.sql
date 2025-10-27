SELECT 'Coupon' AS TableName, COUNT(*) AS RowCount FROM dbo.Coupon
UNION ALL SELECT 'CouponType', COUNT(*) FROM dbo.CouponType
UNION ALL SELECT 'EVoucher', COUNT(*) FROM dbo.EVoucher
UNION ALL SELECT 'EVoucherRedeemLog', COUNT(*) FROM dbo.EVoucherRedeemLog
UNION ALL SELECT 'EVoucherToken', COUNT(*) FROM dbo.EVoucherToken
UNION ALL SELECT 'EVoucherType', COUNT(*) FROM dbo.EVoucherType
UNION ALL SELECT 'MiniGame', COUNT(*) FROM dbo.MiniGame
UNION ALL SELECT 'Pet', COUNT(*) FROM dbo.Pet
UNION ALL SELECT 'PetBackgroundCostSettings', COUNT(*) FROM dbo.PetBackgroundCostSettings
UNION ALL SELECT 'PetLevelRewardSettings', COUNT(*) FROM dbo.PetLevelRewardSettings
UNION ALL SELECT 'PetSkinColorCostSettings', COUNT(*) FROM dbo.PetSkinColorCostSettings
UNION ALL SELECT 'SignInRule', COUNT(*) FROM dbo.SignInRule
UNION ALL SELECT 'SystemSettings', COUNT(*) FROM dbo.SystemSettings
UNION ALL SELECT 'User_Wallet', COUNT(*) FROM dbo.User_Wallet
UNION ALL SELECT 'UserSignInStats', COUNT(*) FROM dbo.UserSignInStats
UNION ALL SELECT 'WalletHistory', COUNT(*) FROM dbo.WalletHistory
UNION ALL SELECT 'ManagerData', COUNT(*) FROM dbo.ManagerData
UNION ALL SELECT 'ManagerRole', COUNT(*) FROM dbo.ManagerRole
UNION ALL SELECT 'ManagerRolePermission', COUNT(*) FROM dbo.ManagerRolePermission
UNION ALL SELECT 'Users', COUNT(*) FROM dbo.Users
ORDER BY TableName;
