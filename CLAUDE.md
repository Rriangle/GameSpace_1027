# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a dual ASP.NET Core 8.0 solution containing two interconnected projects:
- **GameSpace** (Primary) - Main production application at `GameSpace/GameSpace/`
- **GamiPort** (Companion) - Secondary application at `GamiPort/GamiPort/`

Both projects share the same SQL Server database (`GameSpacedatabase`) and implement an **Areas-based MVC architecture** with a three-layer pattern (Presentation → Business Logic → Data Access).

**Current Development Focus:** Admin backend only for MiniGame Area. Public-facing frontend is scaffolded but not actively developed.

## Build & Run Commands

### Building the Projects

```bash
# From repository root
dotnet restore GameSpace/GameSpace/GameSpace.csproj
dotnet build GameSpace/GameSpace/GameSpace.csproj

# For GamiPort
dotnet restore GamiPort/GamiPort/GamiPort.csproj
dotnet build GamiPort/GamiPort/GamiPort.csproj
```

### Running the Applications

```bash
# Run GameSpace (from repository root)
dotnet run --project GameSpace/GameSpace/GameSpace.csproj

# Run GamiPort (from repository root)
dotnet run --project GamiPort/GamiPort/GamiPort.csproj
```

### Database Setup

Both projects require SQL Server with these databases:
- **GameSpacedatabase** - Business logic database
  - Connection: `Data Source=(local)\SQLEXPRESS;Initial Catalog=GameSpacedatabase`
- **aspnet-GameSpace-[GUID]** - ASP.NET Identity database
  - Connection: `Server=(localdb)\mssqllocaldb`

**Important:** Verify SQL Server is running before starting the application.

## Critical Architecture Decisions

### 1. No EF Migrations - Manual Schema Management

**This project does NOT use Entity Framework migrations:**
- Schema is managed directly in SQL Server using SSMS
- All schema definitions are in the `schema/` folder for AI reference only
- **NEVER run `Add-Migration` or `Update-Database`**
- **NEVER modify schema programmatically**
- Use `db.Database.CanConnect()` for connection testing only

### 2. Strict Area Boundaries

**You can ONLY modify files within your assigned Area:**
- For MiniGame Area: `Areas/MiniGame/**` only
- **Exception:** `Program.cs` - May add necessary service registrations for your Area (do not modify other Areas' registrations)
- **DO NOT** modify other Areas, shared layouts (except Area-specific sidebars), or global files

### 3. Admin-Only Development (Current Phase)

- Current work focuses exclusively on **Admin backend**
- Public-facing frontend exists but is not actively developed
- All new features should be Admin controllers, services, and views
- Use SB Admin 2 template for admin UI (do not modify vendor files)

### 4. Dual Authentication Schemes

Two authentication schemes coexist:
1. **Default Identity Cookie** - For regular users
2. **AdminCookie** - For admin panel
   - 4-hour timeout with sliding expiration
   - Login path: `/Login/Index`
   - Claims: `ManagerId`, `IsManager`, permission claims
   - Use: `[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]`

## Areas Structure

### GameSpace Areas

1. **MiniGame** - Most developed area (primary focus)
   - Gaming economy: wallet, coupons, e-vouchers
   - Pet management: leveling, attributes, customization
   - Sign-in rewards system
   - Mini-game records and gameplay
   - **Admin backend complete, no public frontend**

2. **Identity** - ASP.NET Core Identity customization

3. **MemberManagement** - User profiles and member features

4. **social_hub** - Social features with SignalR real-time chat (`/social_hub/chatHub`)

5. **Forum** - Discussion board functionality

6. **OnlineStore** - E-commerce for in-game items

## Service Layer Architecture - Query/Mutation Pattern (CQRS-lite)

**Critical Pattern:** Services are split into read and write operations:

### Query Services (Read-Only)
- Interface naming: `IWalletQueryService`, `IPetQueryService`, `ISignInQueryService`
- Purpose: Admin viewing/querying data
- Return: ViewModels and paged results
- **No state mutations allowed**

### Mutation Services (Write Operations)
- Interface naming: `IWalletMutationService`, `IPetMutationService`, `ISignInMutationService`
- Purpose: Admin modifications (create, update, delete, issue points/coupons)
- Return: Strongly-typed result objects with state tracking
- Include before/after snapshots

### Business Logic Services (User-Facing)
- Interface naming: `IPetService`, `IWalletService`, `IMiniGameService`
- Purpose: Combined read/write for user-facing features (when public frontend is developed)

### Service Registration

All services for MiniGame Area registered in:
- Location: `Areas/MiniGame/config/ServiceExtensions.cs`
- 60+ services registered in 7 logical phases
- Use `AddScoped` for entity-bound services
- **Do not register MiniGame services directly in Program.cs**

### Result Objects Pattern

**Always return result objects instead of throwing exceptions for business logic failures:**

```csharp
public class WalletMutationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int? BalanceBefore { get; set; }
    public int? BalanceAfter { get; set; }
    // ... state tracking fields
}

// For batch operations
public class BatchWalletMutationResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<WalletMutationResult> Results { get; set; }
}
```

## Database Patterns

### Entity Framework Core Models

**Location:**
- GameSpace: `GameSpace/GameSpace/Models/*.cs`
- GamiPort: `GamiPort/GamiPort/Models/*.cs`

**Important principles:**
- Models are manually created and maintained (NOT generated by EF scaffolding)
- Each model corresponds to a table in the database
- Models must match the actual database schema since migrations are not used
- Both projects maintain their own complete set of models (100+ files each)
- **Do not generate models from database** - manually create/update to maintain control

**Key model files:**
- `GameSpacedatabaseContext.cs` - Main DbContext with 70+ DbSet properties
- `ErrorViewModel.cs` - View model for error pages
- Entity models: `User.cs`, `Pet.cs`, `Coupon.cs`, `EVoucher.cs`, `MiniGame.cs`, etc.

**When adding new features:**
1. Manually create the table in SQL Server (via SSMS)
2. Manually create the corresponding model class in `Models/`
3. Add DbSet property to `GameSpacedatabaseContext.cs`
4. Include soft delete fields if applicable

### Soft Delete Pattern (Universal)

**All tables implement logical deletes with these four fields:**
- `IsDeleted` (bit) - Deletion flag
- `DeletedAt` (datetime2) - Deletion timestamp
- `DeletedBy` (int) - FK to `ManagerData.Manager_Id`
- `DeleteReason` (nvarchar(500)) - Reason text

**Always filter by `WHERE IsDeleted = 0`** in queries unless specifically querying deleted records.

### Multi-DbContext Strategy

Two separate DbContexts with distinct responsibilities:
- **ApplicationDbContext** - Identity/User/Role data (ASP.NET Identity infrastructure only)
- **GameSpacedatabaseContext** - All business logic (70+ tables for MiniGame, Forums, Shopping, etc.)

**Never mix identity concerns with business domain in the same DbContext.**

### Foreign Key Patterns in MiniGame Area

- Most tables → `Users.User_ID`
- Admin operations → `ManagerData.Manager_Id`
- Type tables referenced by instance tables (e.g., `CouponType` ← `Coupon`)

### In-Memory Caching for Configuration Rules

Rule-based configuration cached in memory to reduce database queries:
- `IInMemorySignInRuleService` - Sign-in rules
- `InMemoryPetSkinColorCostSettingService` - Pet customization costs
- TTL varies by service (e.g., 30 seconds for mute filter settings)

## Controller Architecture

### Base Controller Pattern

`MiniGameBaseController` provides common functionality for all MiniGame admin controllers:
- `GetCurrentManagerId()` - Extract admin ID from claims
- `GetCurrentManagerAsync()` - Load current admin entity
- `HasPermissionAsync(string permissionName)` - Check granular permissions

**All admin controllers in MiniGame Area must inherit from this base.**

### Controller Naming Conventions

- Admin Controllers: `AdminXxxController` (e.g., `AdminHomeController`, `AdminPetController`)
- Settings Controllers: Nested under `Settings/` folder
- Always use `[Area("MiniGame")]` attribute

### Authorization Pattern

```csharp
[Area("MiniGame")]
[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
public class AdminPetController : MiniGameBaseController
{
    public async Task<IActionResult> Index()
    {
        // Check granular permission
        if (!await HasPermissionAsync("PetManagement"))
        {
            return Forbid();
        }
        // ... implementation
    }
}
```

### Login Redirection

**Do not implement login within MiniGame Area.** Redirect to shared login:

```csharp
[AllowAnonymous]
public IActionResult Login(string? returnUrl = null)
    => RedirectToAction("Index", "Login", new {
        area = "", // Main login is at root level
        returnUrl = Url.Action("Index", "Home", new { area = "MiniGame" })
    });
```

## File Organization

### Models & ViewModels

**Entity Models Location:** `GameSpace/GameSpace/Models/` or `GamiPort/GamiPort/Models/`
- These are EF Core entity models that map to database tables
- Maintained manually (not scaffolded from database)
- Must match actual database schema exactly

**Area-Specific ViewModels Location:** `Areas/MiniGame/Models/`

Naming conventions:
- Entity Models: Direct names matching table names (`Pet.cs`, `User.cs`, `Coupon.cs`)
- Admin ViewModels: `AdminViewModels.cs`, `AdminReadPageViewModels.cs`
- Service DTOs: `ServiceViewModels.cs`
- Settings Models: `Settings/*.cs`

**Important distinction:**
- Entity models in root `Models/` folder = Database tables (used by EF Core)
- ViewModels in `Areas/MiniGame/Models/` = Data transfer objects (used by controllers/views)

Use partial classes for model extensions when needed.

### Constants

Location: `Areas/MiniGame/Constants/`

Feature-based organization:
- `WalletConstants.cs`
- `PetConstants.cs`
- `SignInConstants.cs`
- `CouponConstants.cs`

**Always use constants instead of magic strings/numbers.**

### Views Organization

```
Areas/MiniGame/Views/
├── AdminHome/          → Dashboard
├── AdminCoupon/        → Coupon management
├── AdminPet/           → Pet management
├── AdminMiniGame/      → Game records
├── AdminUser/          → User queries
├── AdminManager/       → Admin management
└── Shared/
    └── _Sidebar.cshtml → MiniGame-specific two-level navigation
```

## MiniGame Admin Navigation Structure

**The `_Sidebar.cshtml` in MiniGame Area must have exactly these second-level buttons (from `schema/README_合併版.md`):**

### 會員錢包 (Member Wallet)
1. 查詢會員點數
2. 查詢會員擁有商城優惠券
3. 查詢會員擁有電子禮券
4. 發放會員點數
5. 發放會員擁有商城優惠券
6. 調整會員擁有電子禮券
7. 查看會員收支明細

### 會員簽到系統 (Sign-In System)
1. 簽到規則設定
2. 查看會員簽到紀錄

### 寵物系統 (Pet System)
1. 整體寵物系統規則設定
2. 會員個別寵物設定
3. 會員個別寵物清單含查詢

### 小遊戲系統 (Mini-Game System)
1. 遊戲規則設定
2. 查看會員遊戲紀錄

**Do not add, remove, or rename these navigation items without updating the specification.**

## Key Tables in MiniGame Area (100% Coverage Required)

**Total: 20 tables** (16 main MiniGame tables + 4 user/permission related tables)
**Reference:** `schema/MiniGameArea相關sql_server_DB相關表格.md`

### MiniGame Area Main Tables (16 tables)

#### Wallet System (2 tables)
- `User_Wallet` (PK: User_Id) - User point balances (User_Point, soft delete fields only)
- `WalletHistory` (PK: LogID) - Transaction history (ChangeType, PointsChanged, ItemCode, Description, ChangeTime)

#### Coupon System (2 tables)
- `CouponType` - Coupon definitions (Name, DiscountType, DiscountValue, MinSpend, ValidFrom, ValidTo, PointsCost)
- `Coupon` - Coupon instances (CouponCode, IsUsed, AcquiredTime, UsedTime, UsedInOrderID, FK: UserId, CouponTypeId)

#### E-Voucher System (4 tables)
- `EVoucherType` - E-voucher definitions (Name, ValueAmount, ValidFrom, ValidTo, PointsCost, TotalAvailable)
- `EVoucher` - E-voucher instances (EVoucherCode, IsUsed, AcquiredTime, UsedTime, FK: UserId, EVoucherTypeId)
- `EVoucherToken` - Redemption tokens (Token, ExpiresAt, IsRevoked, FK: EVoucherId)
- `EVoucherRedeemLog` - Redemption history (Status: REVOKED/REJECTED/EXPIRED/ALREADYUSED/APPROVED)

#### Sign-In System (2 tables)
- `SignInRule` - Configuration/rules (SignInDay, Points, Experience, HasCoupon, CouponTypeCode, IsActive)
- `UserSignInStats` - User sign-in records (SignTime, PointsGained, ExpGained, CouponGained with timestamps)

#### Pet System (4 tables)
- `Pet` (PK: PetID) - Pet main table
  - 5 attributes (0-100 range): Hunger, Mood, Stamina, Cleanliness, Health
  - Level, Experience, CurrentExperience, ExperienceToNextLevel
  - SkinColor (varchar(7) #RRGGBB format), BackgroundColor (nvarchar(20))
  - Change tracking: SkinColorChangedTime, BackgroundColorChangedTime
  - Point tracking: PointsChanged_SkinColor, PointsChanged_BackgroundColor
  - Level rewards: PointsGained_LevelUp, PointsGainedTime_LevelUp, TotalPointsGained_LevelUp
- `PetSkinColorCostSettings` - Skin color pricing (ColorCode, ColorName, PointsCost, Rarity, IsFree, IsLimitedEdition) ✨ **(Added 2025-10-20)**
- `PetBackgroundCostSettings` - Background pricing (BackgroundCode, BackgroundName, PointsCost) ✨ **(Added 2025-10-20)**
- `PetLevelRewardSettings` - Level-up rewards (LevelRangeStart, LevelRangeEnd, PointsReward) ✨ **(Added 2025-10-20)**

#### Mini-Game System (1 table)
- `MiniGame` - Game session records (PK: auto-increment)
  - Game state: Level, MonsterCount, SpeedMultiplier, Result, Aborted
  - Rewards: ExpGained, PointsGained, CouponGained (with timestamps)
  - Pet attribute deltas: HungerDelta, MoodDelta, StaminaDelta, CleanlinessDelta
  - Timing: StartTime, EndTime
  - FK: UserID, PetID

#### System Configuration (1 table)
- `SystemSettings` - Global configuration (SettingKey, SettingValue, Category, SettingType: String/Boolean/Number/JSON, IsReadOnly, IsActive)

### User/Permission Related Tables (4 tables)

These tables have FK relationships with MiniGame Area tables:

- `ManagerData` (PK: Manager_Id) - Admin accounts (Manager_Email, Manager_EmailConfirmed, Manager_AccessFailedCount, Manager_LockoutEnabled, Manager_LockoutEnd)
- `ManagerRole` - Role assignments (FK: Manager_Id, ManagerRole_Id)
- `ManagerRolePermission` - Role definitions with permissions:
  - AdministratorPrivilegesManagement
  - UserStatusManagement (UserManagement)
  - ShoppingPermissionManagement (ShoppingManagement)
  - MessagePermissionManagement (ForumManagement)
  - Pet_Rights_Management (PetManagement)
  - customer_service (CustomerService)
  - (See `schema/管理者權限相關描述.txt` for complete matrix)
- `Users` (PK: User_ID) - User basic information (referenced by most MiniGame tables)

## Schema Documentation (Critical Reference)

**All MiniGame Area work must reference `schema/` folder documentation:**

### Master Specifications
1. **`schema/README_合併版.md`** - Single source of truth
   - Area registration architecture
   - Admin navigation structure (exact button names required)
   - Database coverage requirements (100% for MiniGame tables)
   - Login/cookie integration patterns
   - Verification checklist

2. **`schema/MiniGameArea相關sql_server_DB相關表格.md`**
   - **Total: 20 tables** (16 main MiniGame tables + 4 user/permission related tables)
   - Last verified: 2025-10-27
   - Database server: DESKTOP-8HQIS1S\SQLEXPRESS

3. **`schema/MiniGame_Area_資料庫完整結構文件_2025-10-27.md`** ✨ **(Updated 2025-10-27)**
   - Complete database structure with field definitions (285+ fields)
   - All PK/FK/CHECK Constraints/Indexes/Defaults verified
   - Entity Relationship Diagram (ERD)
   - Field constraints and relationships
   - **Major updates**: Pet table fields, SystemSettings table, new setting tables

4. **`schema/專案規格敘述1.txt` & `schema/專案規格敘述2.txt`** ✨ **(Updated 2025-10-27)**
   - Business requirements (90% of specifications)
   - Technical specifications
   - Performance and security requirements
   - Database field definitions updated to match actual DB

5. **`schema/管理者權限相關描述.txt`** ✨ **(Updated 2025-10-27)**
   - 10 test admin accounts with credentials
   - 8 role definitions with full permissions matrix
   - Complete ManagerData/ManagerRole/ManagerRolePermission table structures
   - Constraint details

6. **`schema/db_schema_summary.md`** ✨ **(New 2025-10-27)**
   - **Authoritative reference** - Queried directly from SQL Server
   - All 20 tables with complete technical details
   - PK/FK/CHECK Constraints/Indexes/Defaults
   - Design patterns (soft delete, audit trail, timestamps)

7. **`schema/資料庫更新總結_2025-10-27.md`** ✨ **(New 2025-10-27)**
   - Complete update report of schema documentation
   - Key findings: All "missing" tables actually exist
   - Database structure 100% complete and verified

### Audit Reports
- `schema/Services修復完成報告_2025-10-21.md` - Service layer fixes (Verified 2025-10-27)
- `schema/SERVICES_AUDIT_FINAL_2025-10-21.md` - Final service audit (Verified 2025-10-27)
- `schema/商業規則差異報告與修正建議.md` - Business rule discrepancies (Verified 2025-10-27)
- `schema/報告驗證更新記錄_2025-10-27.md` - Verification update record ✨ **(New 2025-10-27)**

**Always consult these documents when:**
- Adding new features (understand business rules first)
- Modifying database operations (verify constraints)
- Implementing admin permissions (check role definitions)
- Debugging issues (reference audit reports for known fixes)

## Configuration Files

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-GameSpace-[GUID]",
    "GameSpace": "Data Source=(local)\\SQLEXPRESS;Initial Catalog=GameSpacedatabase;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

**Never commit sensitive data.** Use user secrets for development.

### appsettings.Development.json

Development-specific overrides (logging levels, debug settings).

## UI Framework

### Admin Panel
- **SB Admin 2** template (Bootstrap-based)
- jQuery for legacy functionality
- Vue.js for modern interactive components
- Font Awesome icons
- ClosedXML for Excel exports

**Important:** Template files in `wwwroot/lib/sb-admin/` should NOT be modified. Customize via `wwwroot/css/site.css` only.

### Public Frontend (Future)
- Bootstrap-based (see `schema/index.txt` for specifications)
- Not currently being developed

## Middleware Pipeline Order

From `Program.cs`, the critical order is:
1. `UseSession()` - Before authentication
2. `UseAuthentication()` - Before authorization
3. `UseAuthorization()` - Before routes
4. Area routes: `{area:exists}/{controller=Home}/{action=Index}/{id?}`
5. SignalR hub mapping: `MapHub<ChatHub>("/social_hub/chatHub")`

**Do not reorder** as it will break authentication/authorization.

## Adding New Features to MiniGame Area

### Step-by-Step Process

1. **Database First (Manual in SSMS)**
   - Create tables manually in SQL Server using SSMS
   - Update schema documentation in `schema/` folder
   - Manually create corresponding C# model class in `Models/` folder
     - Use appropriate data types (string, int, DateTime, bool, etc.)
     - Add navigation properties for relationships
     - Include soft delete fields: IsDeleted, DeletedAt, DeletedBy, DeleteReason
   - Add DbSet property to `GameSpacedatabaseContext.cs`:
     ```csharp
     public virtual DbSet<YourModel> YourModels { get; set; }
     ```
   - **DO NOT use `Scaffold-DbContext` or any code generation** - maintain manual control

2. **Service Layer**
   - Create interfaces: `IXxxQueryService`, `IXxxMutationService`
   - Implement services with result object pattern
   - Register in `Areas/MiniGame/config/ServiceExtensions.cs`
   - Use `AddScoped` for entity-bound services

3. **Controllers**
   - Inherit from `MiniGameBaseController`
   - Add `[Area("MiniGame")]` attribute
   - Add `[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]`
   - Check granular permissions with `HasPermissionAsync()`

4. **ViewModels**
   - Add to `Areas/MiniGame/Models/` or `Models/ViewModels/`
   - Use naming convention: `Admin*ViewModel` for admin screens
   - Separate read models from write models

5. **Views**
   - Create folder under `Areas/MiniGame/Views/`
   - Use Razor syntax with Bootstrap 5 classes
   - Reference SB Admin 2 components
   - Update `_Sidebar.cshtml` if adding navigation items (must match specification)

## Development Best Practices

### Query Optimization
- Use `AsNoTracking()` for read-only queries
- Return Read Models/DTOs, not entities
- Implement paging for large result sets (batch limit ≤ 1000)

### Transaction Management
- Use transactions for all state-changing operations (points, coupons, sign-ins, game results)
- Prevent negative balances and concurrency issues
- Implement idempotent operations where possible

### Error Handling
- Use result objects for business logic failures (not exceptions)
- Use `ProblemDetails` or unified Result types for API responses
- Log errors with correlation IDs (Serilog recommended)

### Code Commits
- Keep commits small: ≤ 3 files / ≤ 400 lines per commit
- Write clear commit messages explaining WHY and HOW
- Follow drift repair process when deviating from spec

### File Encoding
- All files must be **UTF-8 with BOM** (especially for Chinese content)

## Testing & Diagnostics

### Database Connection Testing
- Use `IDiagnosticsService.TestDatabaseConnectionAsync()`
- Healthcheck endpoint: `/healthz/db` should return `{"status":"ok"}`
- Do not modify data during connection tests

### Permission Testing
- 8 permission types defined in `ManagerRolePermission`
- Test with different admin roles
- Verify claims are set correctly on login

## Common Pitfalls (Critical)

1. **Don't use EF migrations or scaffolding** - Schema and models are managed manually in SQL Server
2. **Don't use Scaffold-DbContext** - Models are manually created and maintained
3. **Always filter soft-deleted records** - Add `WHERE IsDeleted = 0` to all queries
4. **Use result objects, not exceptions** - Return `*MutationResult` from mutation services
5. **Respect authentication schemes** - Admin controllers MUST use `AuthenticationSchemes = "AdminCookie"`
6. **Don't modify SB Admin template files** - Customize via `site.css` only
7. **Register services in ServiceExtensions.cs** - Not directly in `Program.cs` for MiniGame
8. **Consult schema documentation** - Business rules are in `schema/` folder, not in code comments
9. **Stay within Area boundaries** - Only modify `Areas/MiniGame/**` (except minimal `Program.cs` additions)
10. **Match navigation exactly** - Sidebar buttons must match specification exactly
11. **100% table coverage** - All MiniGame tables must be fully utilized in admin backend
12. **Keep models in sync** - When updating database schema, manually update corresponding model files

## NuGet Packages

### GameSpace (8.0.19)
- Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.AspNetCore.Identity.UI
- Microsoft.AspNetCore.SignalR.Client
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.VisualStudio.Web.CodeGeneration.Design (8.0.7)
- ClosedXML (0.105.0)

### GamiPort (8.0.20)
- Same packages as GameSpace but version 8.0.20

**Maintain consistent package versions within each project.**

## Verification Checklist

Before considering MiniGame Area complete:

- [ ] Can login with `AdminCookie` and access `/MiniGame/Home/Index`
- [ ] Shared `_Sidebar.cshtml` contains MiniGame link
- [ ] MiniGame `_Sidebar.cshtml` has exact second-level buttons from specification
- [ ] All wallet admin functions work (query/issue points, coupons, e-vouchers, view history)
- [ ] Sign-in admin functions work (configure rules, view records)
- [ ] Pet admin functions work (configure rules, modify individual pets, query with change history)
- [ ] Mini-game admin functions work (configure rules including daily limit, view game records)
- [ ] `/healthz/db` returns `{"status":"ok"}`
- [ ] All files saved as UTF-8 with BOM
- [ ] All 20 MiniGame-related tables (16 main + 4 user/permission) have proper coverage in admin backend
- [ ] All admin operations check permissions via `HasPermissionAsync()`
- [ ] Soft delete pattern implemented correctly (queries filter IsDeleted=0)
- [ ] Result objects used instead of exceptions for business logic
