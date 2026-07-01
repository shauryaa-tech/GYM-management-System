# CloudMex Gym Management System

Complete gym ERP web application — members, staff, attendance, memberships, leads, inventory, expenses, payroll, reports, online payments, and WhatsApp lead bot.

---

## Table of Contents

1. [Overview](#overview)
2. [Tech Stack](#tech-stack)
3. [Architecture](#architecture)
4. [Prerequisites](#prerequisites)
5. [Installation & Setup](#installation--setup)
6. [Database Setup](#database-setup)
7. [Configuration](#configuration)
8. [Running the Application](#running-the-application)
9. [Authentication & RBAC](#authentication--rbac)
10. [Modules & Features](#modules--features)
11. [Dashboard & Navbar](#dashboard--navbar)
12. [Payment Gateway Integration](#payment-gateway-integration)
13. [WhatsApp Bot & API Setup](#whatsapp-bot--api-setup)
14. [Public Routes](#public-routes)
15. [Project Structure](#project-structure)
16. [Migrations vs SQL Scripts](#migrations-vs-sql-scripts)
17. [Troubleshooting](#troubleshooting)

---

## Overview

| Item | Detail |
|------|--------|
| **Product** | CloudMex Gym (CLOUDMEX GYM) |
| **Solution** | `GymManagementSystem.sln` |
| **Project** | `GymManagementSystem/GymManagementSystem.csproj` |
| **Framework** | .NET 9.0 (`net9.0`) |
| **App Type** | ASP.NET Core MVC (Razor Views + Controllers) |
| **Database** | Microsoft SQL Server |
| **Default URL** | `http://localhost:5052` |

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core 9 MVC |
| Data Access | ADO.NET (primary) + EF Core 9 (payment gateway only) |
| Database | SQL Server |
| Auth | Session-based login + BCrypt password hashing |
| UI | Bootstrap 5.3, jQuery 3.7, Font Awesome 6.5, SweetAlert2 |
| Payments | Paytm, PhonePe, Razorpay, Cashfree |
| Messaging | Meta WhatsApp Cloud API |

### NuGet Packages

- `BCrypt.Net-Next` — password hashing
- `Microsoft.EntityFrameworkCore.SqlServer` 9.0
- `Microsoft.EntityFrameworkCore.Tools` 9.0
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` 9.0

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers (MVC)                     │
└─────────────────────────┬───────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          ▼                               ▼
┌─────────────────────┐       ┌─────────────────────────┐
│  Repositories       │       │  Services               │
│  (ADO.NET + SQL)    │       │  Payment / WhatsApp     │
│  DbHelper           │       │  Encryption             │
└─────────┬───────────┘       └───────────┬─────────────┘
          │                               │
          ▼                               ▼
┌─────────────────────┐       ┌─────────────────────────┐
│     SQL Server      │       │  ApplicationDbContext   │
│  (All gym tables)   │       │  (PaymentGateways only) │
└─────────────────────┘       └─────────────────────────┘
```

**Key patterns:**

- **Repository per module** — raw SQL via `Data/Queries` + `Data/Repositories`
- **Service layer** — payments, WhatsApp bot, encryption
- **Provider factory** — `PaymentProviderFactory` for multi-gateway payments
- **Session auth** — no ASP.NET Identity; `[Permission]` filter enforces RBAC
- **Settings provider** — WhatsApp settings from DB first, `appsettings.json` fallback

---

## Prerequisites

- **.NET 9 SDK** — [Download](https://dotnet.microsoft.com/download)
- **SQL Server** — Express or full edition
- **SSMS** (SQL Server Management Studio) — database scripts run karne ke liye
- **Visual Studio 2022** or **VS Code** — development
- **ngrok** (optional) — local WhatsApp webhook testing ke liye

---

## Installation & Setup

### 1. Clone / Open Project

```bash
cd c:\Users\HRMex-3\source\repos\GymManagementSystem\GymManagementSystem
```

### 2. Restore Packages

```bash
dotnet restore
```

### 3. Configure Connection String

Edit `GymManagementSystem/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=GymManagement;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 4. Run Database Scripts

See [Database Setup](#database-setup) section below.

### 5. Build & Run

```bash
dotnet build
dotnet run
```

Browser mein kholo: **http://localhost:5052**

Login page: **http://localhost:5052/Account/Login**

---

## Database Setup

### Step 1 — Create Database

SSMS mein run karein:

```sql
CREATE DATABASE GymManagement;
GO
```

### Step 2 — Base Tables (Required First)

`Schema_Updates.sql` in tables par depend karta hai. Pehle ye base tables honi chahiye:

- `RoleMasters`
- `MemberMasters`
- `MembershipPlanMasters`
- `ExerciseMasters`
- `StaffMasters`
- `PermissionMaster`
- `RolePermission`

> Agar fresh database hai aur base tables nahi hain, to pehle unhe create karein ya existing gym database backup restore karein.

### Step 3 — Run SQL Scripts (Order Matters)

Scripts folder: `GymManagementSystem/Database/`

| Order | Script | Kya banata hai |
|-------|--------|----------------|
| 1 | `Schema_Updates.sql` | ExpenseHeads, LeadSources, Vendors, Diets, Equipment, Users, Products, Classes, Attendance, MembershipTransactions, Leads, TrainerAssignments, WorkoutPlans, DietPlans, PTSessions, ClassBookings, StockPurchases, Expenses, StaffAttendance, SalaryProcessing |
| 2 | `Entries_Schema.sql` | StockIssues, EquipmentMaintenances |
| 3 | `Payments_Schema.sql` | Payments table (member payment records) |
| 4 | `PaymentGateway_Schema.sql` | PaymentGateways, PaymentTransactions + indexes |
| 5 | `Reports_Permissions.sql` | Report module permissions (Super Admin ko grant) |
| 6 | `WhatsAppBot_Schema.sql` | WhatsAppBotSessions, lead sources (WhatsApp Bot / Public Form) |
| 7 | `WhatsAppApiSetup_Schema.sql` | WhatsAppApiSettings table, WhatsAppApiSetup & WhatsAppBot permissions |

**SSMS mein:** Har script open karke `GymManagement` database select karke Execute karein.

### Step 4 — Create Admin User

`UserMasters` mein ek user insert karein (BCrypt hash ke saath):

```csharp
// C# snippet — password hash generate karne ke liye
BCrypt.Net.BCrypt.HashPassword("YourPassword123")
```

```sql
INSERT INTO UserMasters (FullName, UserName, PasswordHash, RoleId, IsActive)
VALUES ('Super Admin', 'admin', '<BCrypt_Hash_Here>', 1, 1);
```

> **RoleId = 1** = Super Admin (sab permissions bypass)

---

## Configuration

File: `GymManagementSystem/appsettings.json`

### Connection String

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection |

### Payment Gateway API URLs

| Section | Purpose |
|---------|---------|
| `Paytm` | Sandbox/Production base URLs, transaction paths |
| `PhonePe` | Sandbox/Production base URLs |
| `Razorpay` | Sandbox/Production base URLs |
| `Cashfree` | Sandbox/Production base URLs |

> Gateway credentials (Key ID, Key Secret) **admin UI** se save hote hain — `Masters → Payment Gateway`.

### WhatsApp (Fallback)

| Key | Purpose |
|-----|---------|
| `WhatsApp:Enabled` | Enable/disable fallback |
| `WhatsApp:PhoneNumberId` | Meta Phone Number ID |
| `WhatsApp:BusinessPhone` | Business WhatsApp number |
| `WhatsApp:AccessToken` | Permanent access token |
| `WhatsApp:VerifyToken` | Webhook verify token |
| `WhatsApp:GraphApiVersion` | e.g. `v21.0` |

> Production mein WhatsApp settings **Masters → WhatsApp API Setup** se DB mein save karein. DB settings `appsettings.json` ko override karti hain.

### Session

- Idle timeout: **30 minutes** (`Program.cs`)

---

## Running the Application

```bash
cd GymManagementSystem
dotnet run
```

| URL | Page |
|-----|------|
| `http://localhost:5052` | Home |
| `http://localhost:5052/Account/Login` | Staff Login |
| `http://localhost:5052/Dashboard/Index` | Dashboard (login ke baad) |

**Development profile:** `Properties/launchSettings.json` → port `5052`

---

## Authentication & RBAC

### Login Flow

1. User `/Account/Login` par username + password daalta hai
2. `AccountRepository` BCrypt se password verify karta hai (`UserMasters` table)
3. Session set hoti hai: `FullName`, `UserName`, `RoleId`
4. Redirect → `/Dashboard/Index`

### Logout

`/Account/Logout` — session clear

### Permission System

| Table | Purpose |
|-------|---------|
| `PermissionMaster` | Module list (ModuleName, DisplayName, SortOrder) |
| `RolePermission` | Per-role: CanView, CanAdd, CanEdit, CanDelete, CanExport |
| `RoleMasters` | Roles |
| `UserMasters` | Users linked to roles |

### Enforcement

```csharp
[Permission("ModuleName", "View|Add|Edit|Delete")]
```

| Rule | Behavior |
|------|----------|
| No session | Redirect → `/Account/Login` |
| **RoleId = 1 (Super Admin)** | All permissions bypass |
| No permission | Redirect → `/Home/AccessDenied` |

Sidebar items `PermissionHelper.CanView()` se show/hide hote hain.

### Permission Modules (Complete List)

`Dashboard`, `MembershipPlans`, `MemberMaster`, `StaffMaster`, `ExerciseMaster`, `DietMaster`, `Classes`, `Equipment`, `Products`, `Vendors`, `ExpenseHeads`, `PaymentGateway`, `WhatsAppApiSetup`, `LeadSources`, `UsersRoles`, `MembershipManagement`, `Attendance`, `Payments`, `Leads`, `TrainerAssignment`, `WorkoutPlans`, `PTSessions`, `ClassBookings`, `StockPurchase`, `StockIssue`, `EquipmentMaintenance`, `Expenses`, `StaffAttendance`, `SalaryProcessing`, `Members`, `ReportAttendance`, `ReportExpiry`, `ReportCollections`, `ReportOutstanding`, `ReportProfitLoss`

> **Note:** `DietPlansController` par `[Permission]` attribute nahi hai — controller level par RBAC protected nahi hai.

### Users & Roles Management

**Masters → Users & Roles** — users create/edit, roles assign, permission matrix manage

---

## Modules & Features

### Dashboard

| Route | Feature |
|-------|---------|
| `/Dashboard/Index` | KPI cards, revenue charts, attendance charts, recent members |

---

### Masters

| Menu | Route | Controller | Permission | Description |
|------|-------|------------|------------|-------------|
| Membership Plans | `/MembershipPlans/Index` | MembershipPlansController | MembershipPlans | Plan CRUD — name, duration, price, features |
| Member Master | `/MemberMasters/Index` | MemberMastersController | MemberMaster | Member registration & profile |
| Staff Master | `/StaffMasters/Index` | StaffMastersController | StaffMaster | Trainers, reception, staff CRUD |
| Exercise Master | `/Exercise/Index` | ExerciseController | ExerciseMaster | Exercise library |
| Diet Master | `/DietMaster/Index` | DietMasterController | DietMaster | Food items — calories, protein, etc. |
| Class Master | `/ClassMaster/Index` | ClassMasterController | Classes | Group classes schedule |
| Equipment Master | `/EquipmentMaster/Index` | EquipmentMasterController | Equipment | Gym equipment inventory |
| Products | `/Products/Index` | ProductsController | Products | Supplements, apparel, stock |
| Vendors | `/Vendors/Index` | VendorsController | Vendors | Supplier/vendor master |
| Expense Heads | `/ExpenseHeads/Index` | ExpenseHeadsController | ExpenseHeads | Expense categories |
| Payment Gateway | `/PaymentGateway/Index` | PaymentGatewayController | PaymentGateway | Paytm/PhonePe/Razorpay/Cashfree config |
| WhatsApp API Setup | `/WhatsAppApiSetup/Index` | WhatsAppApiSetupController | WhatsAppApiSetup | Meta WhatsApp Cloud API credentials |
| Lead Sources | `/LeadSources/Index` | LeadSourcesController | LeadSources | Lead source master |
| Users & Roles | `/UsersRoles/Index` | UsersRolesController | UsersRoles | Users, roles, permissions |

---

### Entries

| Menu | Route | Controller | Permission | Description |
|------|-------|------------|------------|-------------|
| Membership Management | `/MembershipManagement/Index` | MembershipManagementController | MembershipManagement | Assign/renew memberships |
| Attendance | `/Attendance/Index` | AttendanceController | Attendance | Member check-in/out |
| Payments | `/Payments/Index` | PaymentsController | Payments | Collect fees, online pay |
| Leads | `/Leads/Index` | LeadsController | Leads | Lead pipeline management |
| WhatsApp Bot | `/WhatsAppBot/Index` | WhatsAppBotController | Leads (View) | Form link share, bot status |
| Trainer Assignment | `/TrainerAssignments/Index` | TrainerAssignmentsController | TrainerAssignment | Assign trainer to member |
| Workout Plans | `/WorkoutPlans/Index` | WorkoutPlansController | WorkoutPlans | Custom workout plans |
| Diet Plans | `/DietPlans/Index` | DietPlansController | — | Custom diet plans |
| PT Sessions | `/PTSessions/Index` | PTSessionsController | PTSessions | Personal training sessions |
| Class Booking | `/ClassBooking/Index` | ClassBookingController | ClassBookings | Book members into classes |
| Stock Purchase | `/StockPurchase/Index` | StockPurchaseController | StockPurchase | Inventory purchase entries |
| Stock Issue | `/StockIssue/Index` | StockIssueController | StockIssue | Issue stock to members/staff |
| Equipment Maintenance | `/EquipmentMaintenance/Index` | EquipmentMaintenanceController | EquipmentMaintenance | Maintenance logs |
| Expenses | `/Expenses/Index` | ExpensesController | Expenses | Daily expense entries |
| Staff Attendance | `/StaffAttendance/Index` | StaffAttendanceController | StaffAttendance | Staff attendance tracking |
| Salary Processing | `/SalaryProcessing/Index` | SalaryProcessingController | SalaryProcessing | Monthly salary processing |

---

### Reports & Members

| Menu | Route | Controller | Permission | Description |
|------|-------|------------|------------|-------------|
| Members | `/Members/Index` | MembersController | Members | Member list & details |
| Attendance Report | `/Reports/Attendance` | ReportsController | ReportAttendance | Date-range attendance |
| Expiry | `/Reports/MembershipExpiry` | ReportsController | ReportExpiry | Upcoming membership expiry |
| Collections | `/Reports/Collections` | ReportsController | ReportCollections | Payment collections report |
| Outstanding | `/Reports/Outstanding` | ReportsController | ReportOutstanding | Pending dues |
| Profit/Loss | `/Reports/ProfitLoss` | ReportsController | ReportProfitLoss | Income vs expense P&L |

---

### Profile

| Route | Feature |
|-------|---------|
| `/Profile/Index` | View/edit profile, photo upload |
| `/Profile/ChangePassword` | Change password |

---

## Dashboard & Navbar

### Dashboard KPIs

- Total Members / Active Members
- Today's Attendance
- Total Revenue (charts)
- Staff count & staff attendance today
- Recent members list
- Monthly revenue chart
- Attendance trend chart

### Navbar Features (`wwwroot/js/navbar.js`)

| Feature | API Endpoint | Description |
|---------|--------------|-------------|
| Global Search | `GET /Dashboard/GlobalSearch?q=` | Members, leads, staff search (min 2 chars) |
| Notifications | `GET /Dashboard/GetNavbarStats` | Alerts with links |
| Messages | `GET /Dashboard/GetNavbarMessages` | Lead follow-up reminders |
| Today's Revenue | `GET /Dashboard/GetNavbarStats` | Live revenue counter |
| Quick Settings | Dropdown | Profile, Password, Dashboard, Users, Payment Gateway, Logout |

---

## Payment Gateway Integration

### Supported Gateways

| Gateway | Environment | Admin Form Fields |
|---------|-------------|-------------------|
| Paytm | Sandbox / Production | Key ID + Key Secret (auto-filled callback URLs) |
| PhonePe | Sandbox / Production | Key ID + Key Secret |
| Razorpay | Sandbox / Production | Key ID + Key Secret |
| Cashfree | Sandbox / Production | Key ID + Key Secret |

### Admin Setup

1. **Masters → Payment Gateway → Add**
2. Gateway select karein (Paytm / PhonePe / Razorpay / Cashfree)
3. Environment select karein (Sandbox / Production)
4. **Key ID** aur **Key Secret** daalein
5. **Validate API** click karein (optional)
6. Save karein
7. Ek gateway ko **Default** set karein

> Merchant keys encrypted store hote hain (`IEncryptionService` + ASP.NET Data Protection).

### Payment Flows

#### 1. In-App Member Payment

```
Payments → Pay Online → /OnlinePayment/Checkout → Gateway → Callback → Payments table
```

#### 2. WhatsApp Bot / Public Lead Payment

```
Bot sends link → /Pay/Lead?token=xxx → /OnlinePayment/Checkout → Callback
→ Lead converted + WhatsApp confirmation message
```

### Public Callback URLs

| Gateway | Endpoint |
|---------|----------|
| Paytm | `POST /OnlinePayment/PaytmCallback` |
| PhonePe | `POST /OnlinePayment/PhonePeCallback` |
| Razorpay | `POST /OnlinePayment/RazorpayCallback` |
| Cashfree | `POST /OnlinePayment/CashfreeCallback` |
| Cashfree Return | `GET /OnlinePayment/CashfreeReturn` |
| Legacy Paytm | `POST /PaytmPayment/Callback` |

Checkout page: `GET /OnlinePayment/Checkout`

---

## WhatsApp Bot & API Setup

### Part 1 — WhatsApp API Setup (Admin)

**Masters → WhatsApp API Setup** (`/WhatsAppApiSetup/Index`)

| Field | Description |
|-------|-------------|
| Enable WhatsApp API | On/off toggle |
| Phone Number ID | Meta Developer Console se |
| Business WhatsApp Number | e.g. `9876543210` |
| WABA ID | WhatsApp Business Account ID |
| Meta App ID | Facebook App ID |
| Permanent Access Token | System user token (encrypted in DB) |
| Webhook Verify Token | Meta webhook setup mein same token |
| Graph API Version | Default: `v21.0` |
| Welcome Message | Bot welcome text |

**Actions:**
- **Save Settings** — DB mein save (token encrypted)
- **Test Connection** — Meta API se credentials verify

**Webhook URL (copy from page):**
```
https://your-domain.com/api/whatsapp/webhook
```

Local testing ke liye **ngrok** use karein:
```bash
ngrok http 5052
```
Ngrok URL ko Meta webhook mein paste karein.

---

### Part 2 — WhatsApp Bot (Admin)

**Entries → WhatsApp Bot** (`/WhatsAppBot/Index`)

- Public join form link copy/share
- WhatsApp share button
- Webhook URL display
- API configuration status

---

### Part 3 — User Flow (End-to-End)

```
Admin shares /join link
        ↓
User fills form (name, phone, email, etc.)
        ↓
New Lead created in system
        ↓
WhatsApp bot starts conversation
        ↓
Step 1: Select Trainer (interactive list)
        ↓
Step 2: Select Class
        ↓
Step 3: Select Membership Plan
        ↓
Step 4: Pay Now link → /Pay/Lead?token=xxx
        ↓
Online payment (default gateway)
        ↓
Lead marked Converted + WhatsApp confirmation
```

### Public Join Form

| Route | Description |
|-------|-------------|
| `GET /join` | Public lead capture form |
| `POST /join` | Submit form → create lead + start bot session |
| `GET /join/success` | Thank you page + WhatsApp deep link |

### WhatsApp Webhook

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/whatsapp/webhook` | Meta webhook verification |
| POST | `/api/whatsapp/webhook` | Incoming messages → bot handler |

**Meta webhook subscribe:** `messages`

### Bot State

Conversation state `WhatsAppBotSessions` table mein store hoti hai:
- Current step (trainer / class / plan / payment)
- Selected trainer, class, plan IDs
- Payment token

---

## Public Routes

Ye routes **staff login ke bina** accessible hain:

| Route | Method | Controller | Notes |
|-------|--------|------------|-------|
| `/Account/Login` | GET/POST | Account | Staff login |
| `/join` | GET/POST | PublicJoin | Public lead form |
| `/join/success` | GET | PublicJoin | Post-submit page |
| `/Pay/Lead?token=` | GET | Pay | Lead payment start |
| `/OnlinePayment/Checkout` | GET | OnlinePayment | Payment checkout UI |
| `/OnlinePayment/PaytmCallback` | POST | OnlinePayment | Paytm callback |
| `/OnlinePayment/PhonePeCallback` | POST | OnlinePayment | PhonePe callback |
| `/OnlinePayment/RazorpayCallback` | POST | OnlinePayment | Razorpay callback |
| `/OnlinePayment/CashfreeCallback` | POST | OnlinePayment | Cashfree callback |
| `/OnlinePayment/CashfreeReturn` | GET | OnlinePayment | Cashfree return |
| `/PaytmPayment/Callback` | POST | PaytmPayment | Legacy Paytm |
| `/PaytmPayment/Checkout` | GET | PaytmPayment | Redirect to OnlinePayment |
| `/api/whatsapp/webhook` | GET/POST | WhatsAppWebhook | Meta webhook |
| `/Home/Index` | GET | Home | Landing page |
| `/Home/AccessDenied` | GET | Home | Permission denied |

---

## Project Structure

```
GymManagementSystem/
├── GymManagementSystem.sln
├── README.md                          ← This file
└── GymManagementSystem/
    ├── Program.cs                     # DI registration, middleware, routing
    ├── appsettings.json               # Connection string, gateway URLs, WhatsApp fallback
    ├── appsettings.Development.json
    ├── GymManagementSystem.csproj
    │
    ├── Controllers/                   # 41 MVC controllers
    │   ├── AccountController.cs
    │   ├── DashboardController.cs
    │   ├── PaymentGatewayController.cs
    │   ├── WhatsAppApiSetupController.cs
    │   ├── WhatsAppBotController.cs
    │   ├── WhatsAppWebhookController.cs
    │   ├── PublicJoinController.cs
    │   ├── OnlinePaymentController.cs
    │   └── ... (masters, entries, reports)
    │
    ├── Views/                         # Razor views per module
    │   ├── Shared/_Layout.cshtml      # Sidebar, navbar, layout
    │   ├── Dashboard/
    │   ├── PaymentGateway/
    │   ├── WhatsAppApiSetup/
    │   ├── WhatsAppBot/
    │   ├── PublicJoin/
    │   └── ...
    │
    ├── Models/                        # Domain models
    │   └── Entities/                  # EF entities (payment gateway)
    │
    ├── ViewModels/                    # Form & list view models
    │
    ├── Data/
    │   ├── ApplicationDbContext.cs    # EF Core (payment gateway only)
    │   ├── DbHelper.cs                # SQL connection factory
    │   ├── Configurations/            # EF entity configurations
    │   ├── Queries/                   # Raw SQL query strings
    │   └── Repositories/              # 40+ repository classes
    │
    ├── Services/
    │   ├── PaymentGatewayService.cs
    │   ├── EncryptionService.cs
    │   ├── Interfaces/
    │   ├── Paytm/                     # Paytm checksum helpers
    │   ├── PaymentProviders/          # Paytm, PhonePe, Razorpay, Cashfree
    │   └── WhatsApp/                  # Bot, API, settings provider
    │
    ├── Helpers/
    │   ├── PermissionAttribute.cs     # RBAC action filter
    │   ├── PermissionHelper.cs        # Sidebar permission checks
    │   └── CallbackUrlAttribute.cs    # Payment callback URL validation
    │
    ├── Database/                      # Manual SQL scripts (7 files)
    │   ├── Schema_Updates.sql
    │   ├── Entries_Schema.sql
    │   ├── Payments_Schema.sql
    │   ├── PaymentGateway_Schema.sql
    │   ├── Reports_Permissions.sql
    │   ├── WhatsAppBot_Schema.sql
    │   └── WhatsAppApiSetup_Schema.sql
    │
    ├── Migrations/                    # EF migration (payment gateway only)
    │   └── 20250623120000_AddPaymentGatewayModule.cs
    │
    ├── Properties/
    │   └── launchSettings.json        # Port 5052
    │
    └── wwwroot/
        ├── js/
        │   ├── sidebar-nav.js
        │   ├── navbar.js
        │   └── payment-gateway.js
        ├── css/
        └── uploads/Profiles/          # User profile photos
```

---

## Migrations vs SQL Scripts

| Approach | Scope | When to Use |
|----------|-------|-------------|
| **SQL Scripts** (`Database/`) | Full gym schema, permissions, WhatsApp, payments | **Primary / Recommended** — most modules use ADO.NET |
| **EF Core Migrations** (`Migrations/`) | Only `PaymentGateways` + `PaymentTransactions` | Alternative for payment gateway tables |

```bash
# EF migration alternative (payment gateway only)
dotnet ef database update --project GymManagementSystem
```

> **Important:** `dotnet ef database update` sirf payment gateway tables banata hai. Leads, Members, Attendance, etc. ke liye SQL scripts zaroori hain.

---

## Troubleshooting

### Build Error — File Locked

Agar app already run ho rahi hai:
```bash
# Running app band karein, phir:
dotnet build
```

Ya compile-only:
```bash
dotnet msbuild /t:CoreCompile
```

### Database Connection Failed

- SQL Server service running hai check karein
- `appsettings.json` mein server name sahi hai verify karein
- `TrustServerCertificate=True` add karein agar SSL error aaye

### Login Failed

- `UserMasters` mein user exist karta hai check karein
- Password BCrypt hash se match hona chahiye
- `IsActive = 1` hona chahiye

### Payment Gateway Save Failed

- Key ID aur Key Secret required hain
- Callback URL localhost allow hai (`CallbackUrlAttribute`)
- Sandbox credentials sandbox environment ke saath use karein

### WhatsApp Messages Not Sending

1. **Masters → WhatsApp API Setup** mein settings save karein
2. **Enable WhatsApp API** toggle ON karein
3. **Test Connection** pass hona chahiye
4. Meta webhook verified aur `messages` subscribed hona chahiye
5. Local dev ke liye ngrok public URL use karein

### WhatsApp Webhook Verification Failed

- Verify Token form mein jo save kiya wahi Meta console mein daalein
- Webhook URL publicly accessible honi chahiye (localhost direct kaam nahi karega)

### Permission Denied / Menu Not Visible

- **Users & Roles** se role ko module permission dein
- Super Admin (`RoleId = 1`) ko sab access milta hai
- `Reports_Permissions.sql` aur `WhatsAppApiSetup_Schema.sql` run karein agar naye modules dikhte nahi

### SQL Script Error — Object Already Exists

Scripts idempotent nahi hain (`Schema_Updates.sql`). Fresh database par run karein ya `IF NOT EXISTS` checks manually add karein.

---

## Quick Reference — All Controllers

| # | Controller | Primary Route |
|---|------------|---------------|
| 1 | AccountController | `/Account/Login` |
| 2 | HomeController | `/Home/Index` |
| 3 | DashboardController | `/Dashboard/Index` |
| 4 | ProfileController | `/Profile/Index` |
| 5 | MembershipPlansController | `/MembershipPlans/Index` |
| 6 | MemberMastersController | `/MemberMasters/Index` |
| 7 | StaffMastersController | `/StaffMasters/Index` |
| 8 | ExerciseController | `/Exercise/Index` |
| 9 | DietMasterController | `/DietMaster/Index` |
| 10 | ClassMasterController | `/ClassMaster/Index` |
| 11 | EquipmentMasterController | `/EquipmentMaster/Index` |
| 12 | ProductsController | `/Products/Index` |
| 13 | VendorsController | `/Vendors/Index` |
| 14 | ExpenseHeadsController | `/ExpenseHeads/Index` |
| 15 | PaymentGatewayController | `/PaymentGateway/Index` |
| 16 | WhatsAppApiSetupController | `/WhatsAppApiSetup/Index` |
| 17 | LeadSourcesController | `/LeadSources/Index` |
| 18 | UsersRolesController | `/UsersRoles/Index` |
| 19 | MembershipManagementController | `/MembershipManagement/Index` |
| 20 | AttendanceController | `/Attendance/Index` |
| 21 | PaymentsController | `/Payments/Index` |
| 22 | LeadsController | `/Leads/Index` |
| 23 | WhatsAppBotController | `/WhatsAppBot/Index` |
| 24 | TrainerAssignmentsController | `/TrainerAssignments/Index` |
| 25 | WorkoutPlansController | `/WorkoutPlans/Index` |
| 26 | DietPlansController | `/DietPlans/Index` |
| 27 | PTSessionsController | `/PTSessions/Index` |
| 28 | ClassBookingController | `/ClassBooking/Index` |
| 29 | StockPurchaseController | `/StockPurchase/Index` |
| 30 | StockIssueController | `/StockIssue/Index` |
| 31 | EquipmentMaintenanceController | `/EquipmentMaintenance/Index` |
| 32 | ExpensesController | `/Expenses/Index` |
| 33 | StaffAttendanceController | `/StaffAttendance/Index` |
| 34 | SalaryProcessingController | `/SalaryProcessing/Index` |
| 35 | MembersController | `/Members/Index` |
| 36 | ReportsController | `/Reports/Attendance` |
| 37 | PublicJoinController | `/join` |
| 38 | PayController | `/Pay/Lead` |
| 39 | OnlinePaymentController | `/OnlinePayment/Checkout` |
| 40 | PaytmPaymentController | `/PaytmPayment/Callback` |
| 41 | WhatsAppWebhookController | `/api/whatsapp/webhook` |

---

## License

Internal / proprietary — CloudMex Gym Management System.

---

*Last updated: June 2025*
#   G Y M - m a n a g e m e n t - S y S t e m  
 