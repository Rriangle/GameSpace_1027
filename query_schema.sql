-- 查詢所有20個表格的完整結構資訊

-- 表格清單
DECLARE @tables TABLE (TableName NVARCHAR(128))
INSERT INTO @tables VALUES
('Coupon'), ('CouponType'), ('EVoucher'), ('EVoucherRedeemLog'), ('EVoucherToken'), ('EVoucherType'),
('MiniGame'), ('Pet'), ('PetBackgroundCostSettings'), ('PetLevelRewardSettings'), ('PetSkinColorCostSettings'),
('SignInRule'), ('SystemSettings'), ('User_Wallet'), ('UserSignInStats'), ('WalletHistory'),
('ManagerData'), ('ManagerRole'), ('ManagerRolePermission'), ('Users');

-- 1. 表格欄位資訊
SELECT
    t.name AS TableName,
    c.name AS ColumnName,
    c.column_id AS OrdinalPosition,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.precision AS NumericPrecision,
    c.scale AS NumericScale,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    dc.definition AS DefaultValue,
    CAST(ep.value AS NVARCHAR(500)) AS ColumnDescription
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
LEFT JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description'
WHERE t.name IN (SELECT TableName FROM @tables)
ORDER BY t.name, c.column_id;

-- 2. 主鍵資訊
SELECT
    t.name AS TableName,
    i.name AS PrimaryKeyName,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS PKColumns
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.is_primary_key = 1 AND t.name IN (SELECT TableName FROM @tables)
GROUP BY t.name, i.name
ORDER BY t.name;

-- 3. 外鍵資訊
SELECT
    t.name AS TableName,
    fk.name AS ForeignKeyName,
    c.name AS ColumnName,
    rt.name AS ReferencedTable,
    rc.name AS ReferencedColumn,
    fk.delete_referential_action_desc AS OnDelete,
    fk.update_referential_action_desc AS OnUpdate
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
INNER JOIN sys.tables rt ON fkc.referenced_object_id = rt.object_id
INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
WHERE t.name IN (SELECT TableName FROM @tables)
ORDER BY t.name, fk.name;

-- 4. CHECK Constraints
SELECT
    t.name AS TableName,
    cc.name AS ConstraintName,
    cc.definition AS CheckDefinition
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON cc.parent_object_id = t.object_id
WHERE t.name IN (SELECT TableName FROM @tables)
ORDER BY t.name;

-- 5. UNIQUE Constraints 和索引
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS IndexColumns
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE t.name IN (SELECT TableName FROM @tables) AND i.is_primary_key = 0
GROUP BY t.name, i.name, i.type_desc, i.is_unique
ORDER BY t.name, i.name;

-- 6. 資料筆數統計
SELECT
    t.name AS TableName,
    SUM(p.rows) AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE t.name IN (SELECT TableName FROM @tables) AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;
