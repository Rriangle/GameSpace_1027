# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a dual ASP.NET Core 8.0 solution containing two interconnected projects:
- **GameSpace** (Primary) - Main production application at `GameSpace/GameSpace/`
- **GamiPort** (Companion) - Secondary application at `GamiPort/GamiPort/`

Both projects share the same SQL Server database (`GameSpacedatabase`) and implement an **Areas-based MVC architecture** with a three-layer pattern (Presentation → Business Logic → Data Access).

## Build & Run Commands

### Building the Projects

```bash
# Restore dependencies
dotnet restore GameSpace/GameSpace/GameSpace.csproj
dotnet restore GamiPort/GamiPort/GamiPort.csproj

# Build GameSpace
dotnet build GameSpace/GameSpace/GameSpace.csproj

# Build GamiPort
dotnet build GamiPort/GamiPort/GamiPort.csproj
```

### Running the Applications

```bash
# Run GameSpace (from repository root)
dotnet run --project GameSpace/GameSpace/GameSpace.csproj

# Run GamiPort (from repository root)
dotnet run --project GamiPort/GamiPort/GamiPort.csproj
```

### Database Requirements

Both projects require SQL Server with these databases:
- **GameSpacedatabase** - Business logic (connection string in appsettings.json uses `(local)\SQLEXPRESS`)
- **aspnet-GameSpace-[GUID]** - ASP.NET Identity (uses `(localdb)\mssqllocaldb`)

Verify SQL Server is running before starting the application.

## Architecture Overview

### Areas Structure

The codebase uses ASP.NET Core Areas for feature modularity:

**GameSpace Areas:**
1. **MiniGame** - Primary focus, most developed area
   - Gaming economy system with wallet, coupons, e-vouchers
   - Pet management system with leveling/attributes
   - Sign-in rewards system
   - Mini-game records and gameplay
   - Complete admin backend (no public frontend yet)

2. **Identity** - ASP.NET Core Identity customization for user authentication

3. **MemberManagement** - User profiles and member-related features

4. **social_hub** - Social features with SignalR-based real-time chat (`/social_hub/chatHub`)

5. **Forum** - Discussion board functionality

6. **OnlineStore** - E-commerce for in-game items/merchandise

### Multi-DbContext Strategy

Two separate DbContexts with distinct purposes:
- **ApplicationDbContext** - Identity/User/Role data (ASP.NET Identity infrastructure)
- **GameSpacedatabaseContext** - All business logic (70+ tables for MiniGame, Forums, Shopping, etc.)

**Important:** Never mix identity concerns with business domain in the same DbContext.

### Dual Authentication Schemes

The application uses two authentication schemes that coexist:

1. **Default Identity Cookie** - For regular users
2. **AdminCookie** - For admin panel (4-hour timeout, sliding expiration)
   - Login path: `/Login`
   - Claims: `ManagerId`, `IsManager`, permission claims (`perm:Shopping`, etc.)
   - Policy: `[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]`

**AJAX Authentication Handling:** Controllers return 401/403 status codes for AJAX requests instead of redirecting, allowing frontend to handle authentication gracefully.

## Service Layer Architecture

### Query/Mutation Separation Pattern (CQRS-lite)

Services are split into read and write operations:

**Query Services** - Read-only operations (Admin viewing data)
- Example: `IWalletQueryService`, `IPetQueryService`, `ISignInQueryService`
- Return ViewModels and paged results
- No state mutations

**Mutation Services** - Write operations (Admin modifications)
- Example: `IWalletMutationService`, `IPetMutationService`, `ISignInMutationService`
- Return strongly-typed result objects with detailed state tracking
- Include before/after snapshots

**Business Logic Services** - Combined read/write for user-facing features
- Example: `IPetService`, `IWalletService`, `IMiniGameService`
- Used by public controllers (when implemented)

### Service Registration

All services for an Area are registered in centralized configuration:
- Location: `Areas/MiniGame/config/ServiceExtensions.cs`
- 60+ services registered in 7 logical phases
- Use `AddScoped` for entity-bound services

### Result Objects Pattern

Operations return standardized result objects instead of throwing exceptions:

```csharp
public class WalletMutationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int? BalanceBefore { get; set; }
    public int? BalanceAfter { get; set; }
    // ... includes state tracking
}

public class BatchWalletMutationResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<WalletMutationResult> Results { get; set; }
}
```

Always return these result objects from mutation services for consistent error handling and operation tracking.

### In-Memory Caching for Configuration Rules

Rule-based configuration is cached in memory to reduce database queries:
- `IInMemorySignInRuleService` - Sign-in rules
- `InMemoryPetSkinColorCostSettingService` - Pet customization costs
- TTL varies by service (e.g., 30 seconds for mute filter settings)

## Database Patterns

### Soft Delete Pattern

All tables implement logical deletes with these fields:
- `IsDeleted` (bit) - Deletion flag
- `DeletedAt` (datetime2) - Deletion timestamp
- `DeletedBy` (int) - FK to ManagerData.Manager_Id
- `DeleteReason` (nvarchar(500)) - Reason text

**Always filter by `WHERE IsDeleted = 0`** in queries unless specifically querying deleted records.

### No EF Migrations

This project does NOT use Entity Framework migrations:
- Schema is managed directly in SQL Server
- Manual DDL changes
- Use `db.Database.CanConnect()` for connection testing only
- Do not generate or run migrations

### Foreign Key Relationships

Key FK patterns in MiniGame Area:
- Most tables → `Users.User_ID`
- Admin operations → `ManagerData.Manager_Id`
- Type tables referenced by instance tables (e.g., `CouponType` ← `Coupon`)

## Controller Architecture

### Base Controller Pattern

`MiniGameBaseController` provides common functionality:
- `GetCurrentManagerId()` - Extract admin ID from claims
- `GetCurrentManagerAsync()` - Load current admin entity
- `HasPermissionAsync(string permissionName)` - Check granular permissions

All admin controllers in MiniGame Area should inherit from this base.

### Controller Naming Conventions

- Admin Controllers: `AdminXxxController` (e.g., `AdminHomeController`, `AdminPetController`)
- Settings Controllers: Nested under `Settings/` folder
- Use `[Area("MiniGame")]` attribute for area routing

### Authorization Pattern

```csharp
[Area("MiniGame")]
[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
public class AdminPetController : MiniGameBaseController
{
    // Permission check example:
    if (!await HasPermissionAsync("PetManagement"))
    {
        return Forbid();
    }
}
```

## File Organization

### Models & ViewModels

Location: `Areas/MiniGame/Models/`

Naming conventions:
- Domain Models: Direct names (`Pet.cs`, `User.cs`, `Coupon.cs`)
- Admin ViewModels: `AdminViewModels.cs`, `AdminReadPageViewModels.cs`
- Service DTOs: `ServiceViewModels.cs`
- Settings Models: `Settings/*.cs`

Use partial classes pattern for model extensions.

### Constants

Location: `Areas/MiniGame/Constants/`

Constants are organized by feature:
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
└── Shared/             → Layouts (_Layout.cshtml)
```

## SignalR Integration

Real-time chat is implemented via SignalR:
- Hub path: `/social_hub/chatHub`
- Hub class: `ChatHub` in social_hub Area
- Transports: WebSockets → Server-Sent Events → Long Polling
- Keep-alive interval: 15 seconds
- Client timeout: 60 seconds

Configure CORS for SignalR in `Program.cs` if needed.

## Important Schema Documentation

Comprehensive database and business rules documentation is in `schema/` folder:

**Critical documents:**
1. `README_合併版.md` - Master specification (Area registration, admin navigation, feature matrix)
2. `MiniGame_Area_資料庫完整結構文件_2025-10-21.md` - Complete DB structure (18 tables, 241 fields, ERD)
3. `專案規格敘述1.txt` & `專案規格敘述2.txt` - Business requirements and technical specs
4. `管理者權限相關描述.txt` - Admin roles/permissions matrix (8 roles defined)

**Audit reports:**
- `Services修復完成報告_2025-10-21.md` - Service layer audit results
- `SERVICES_AUDIT_FINAL_2025-10-21.md` - Final service audit
- `商業規則差異報告與修正建議.md` - Business rule discrepancy analysis

**Always consult these documents when:**
- Adding new features to understand business rules
- Modifying database schema to verify constraints
- Implementing admin permissions to check role definitions

## Key Tables in MiniGame Area

**Wallet System:**
- `User_Wallet` (PK: User_Id) - User point balances
- `WalletHistory` (PK: LogID) - Transaction history

**Coupon System:**
- `CouponType` - Coupon definitions
- `Coupon` - Coupon instances (FK: UserId, CouponTypeId)

**E-Voucher System:**
- `EVoucherType` - E-voucher definitions
- `EVoucher` - E-voucher instances
- `EVoucherToken` - Redemption tokens
- `EVoucherRedeemLog` - Redemption history

**Pet System:**
- `Pet` (PK: PetID) - Pet main table with 5 attributes (Hunger, Mood, Energy, Cleanliness, Health)
- `PetSkinColorCostSettings` - Skin color pricing
- `PetBackgroundCostSettings` - Background pricing
- `PetLevelRewardSettings` - Level-up rewards

**Sign-In System:**
- `SignInRule` - Configuration/rules
- `UserSignInStats` - User sign-in records

**Mini-Game System:**
- `MiniGame` - Game session records (FK: UserID, PetID)

**Admin System:**
- `ManagerData` (PK: Manager_Id) - Admin accounts
- `ManagerRole` - Role assignments
- `ManagerRolePermission` - 8 permission types (AdministratorPrivilegesManagement, UserManagement, ShoppingManagement, etc.)

## Configuration Files

**appsettings.json** contains:
- Connection strings for both databases
- SMTP email configuration (Gmail)
- Logging levels
- CORS settings

**appsettings.Development.json** - Development-specific overrides

**Never commit sensitive data like SMTP passwords.** Use user secrets for development.

## UI Framework

**Admin Panel:**
- SB Admin 2 template (Bootstrap-based)
- jQuery for legacy functionality
- Vue.js for modern interactive components
- Font Awesome icons
- ClosedXML for Excel exports

**Important:** The SB Admin 2 template files in `wwwroot/lib/sb-admin/` should not be modified. Customize via `wwwroot/css/site.css` instead.

## Middleware Pipeline Order

From `Program.cs`, the middleware order is:
1. `UseSession()` - Before authentication
2. `UseAuthentication()` - Before authorization
3. `UseAuthorization()` - Before routes
4. Area routes (with `{area:exists}` constraint)
5. SignalR hub mapping (`MapHub<ChatHub>`)

**Do not reorder these** as it will break authentication/authorization.

## Development Notes

### Current Development Focus

The MiniGame Area is **admin backend only**. Public-facing frontend is scaffolded but not actively developed. Focus all new feature work on:
- Admin controllers (`Admin*Controller`)
- Query/Mutation services
- Admin ViewModels
- Admin views

### Adding New Features to MiniGame Area

1. **Database First:**
   - Manually create tables in SQL Server
   - Add DbSet properties to `GameSpacedatabaseContext.cs`
   - Create domain models with soft delete fields

2. **Service Layer:**
   - Create interfaces: `IXxxQueryService`, `IXxxMutationService`
   - Implement services with result object pattern
   - Register in `ServiceExtensions.cs`

3. **Controllers:**
   - Inherit from `MiniGameBaseController`
   - Use `[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]`
   - Check permissions with `HasPermissionAsync()`

4. **ViewModels:**
   - Add to appropriate file in `Models/` or `Models/ViewModels/`
   - Use naming convention: `Admin*ViewModel` for admin screens

5. **Views:**
   - Create folder under `Areas/MiniGame/Views/`
   - Use Razor syntax with Bootstrap 5 classes
   - Reference SB Admin 2 components where appropriate

### Testing Database Connections

Use diagnostic controllers like `MiniGameDiagnosticsController` with:
- `IDiagnosticsService.TestDatabaseConnectionAsync()` - Tests connection
- Returns success/failure without modifying data

### Permission System

8 permission types in `ManagerRolePermission`:
1. AdministratorPrivilegesManagement
2. UserManagement
3. ShoppingManagement
4. ForumManagement
5. PetManagement
6. CustomerService
7. (2 more - see `管理者權限相關描述.txt`)

Check permissions in controllers before allowing operations.

## Common Pitfalls

1. **Don't use EF migrations** - Schema is managed manually in SQL Server
2. **Always filter soft-deleted records** - Add `WHERE IsDeleted = 0` to queries
3. **Use result objects, not exceptions** - Return `*MutationResult` from mutation services
4. **Respect authentication schemes** - Admin controllers must use `AuthenticationSchemes = "AdminCookie"`
5. **Don't modify SB Admin template files** - Customize via site.css instead
6. **Register services in ServiceExtensions.cs** - Don't register directly in Program.cs for MiniGame services
7. **Consult schema documentation** - Business rules are documented in `schema/` folder

## NuGet Packages

Key dependencies:
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.19/8.0.20
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.19/8.0.20
- `Microsoft.AspNetCore.SignalR.Client` 8.0.19
- `ClosedXML` 0.105.0 - Excel export functionality

Use consistent package versions across both projects.
