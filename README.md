# DepiR5gp - ASP.NET Core MVC Project

Welcome to the **DepiR5gp** repository! This document outlines our collaborative branching strategy, workflow rules, and a quick-start guide for setting up your local environment.

---

## 🌿 Branching Strategy & Workflow

We use a strict **Git Branching Strategy** to ensure code stability and automated continuous deployment to Azure.

### The Branch Rules
1. **`deploy` (Production Branch)**
    * **Never touch or commit to this branch directly.**
    * This branch is strictly controlled by the repository i neverr touched by anyone  to trigger automated deployments to Azure. It only ever receives fast-forward merges from `main`.
2. **`main` (Integration Branch)**
    * **No direct commits are allowed on `main`.**
    * This is our core integration branch where all finished features come together.
3. **`feature/*` (Feature Branches)**
    * Every new feature, page, or bug fix must be built in its own isolated branch created off `main`.
    * **Naming Convention:** `feature/<your-feature-name>` (e.g., `feature/user-authentication`, `feature/product-dashboard`).
4. **Pull Requests (PRs)**
    * Code can only enter `main` via a **Pull Request** from a `feature/*` branch. Once reviewed and tested, the PR is merged into `main`.

### Visual Workflow Diagram

```text
feature/auth      o---o (Work & Commit locally)
                 /     \ 
main    o-------o-------o-------o  <-- (PR Merge only, No direct commits)
                                 \ (Fast-Forward Merge)
deploy  o-------------------------o  <-- (Azure Auto-Deploy Trigger)
```

---

##  Getting Started Guide (Step-by-Step)

Follow these steps when you first clone the repository or set up your local development environment.

### Step 1: Clone the Repository if you dont have it local already 
Open your terminal and clone the project:
```bash
git clone https://github.com/Mahmoud12344/depiR5gp.git
cd depiR5gp
```

### Step 2: Restore Dependencies
Restore all NuGet packages locked by our Central Package Management (`Directory.Packages.props`):
```bash
dotnet restore
```

### Step 3: Configure Database & Secrets 
We use .NET User Secrets to handle cross-platform database connections securely without committing passwords to Git.

### *LINUX ONLY*
* **If you are on Linux / Docker (Fedora, Ubuntu, etc.):**
  Run the automated setup script in the root folder:
  ```bash
  chmod +x setup-linux-secrets.sh
  ./setup-linux-secrets.sh
  ```
* **If you are on Windows (Visual Studio / LocalDB):**
  Windows teammates use `appsettings.json` natively via `Trusted_Connection=True` and do not require manual secrets configuration.

### Step 4: Apply Database Migrations
Ensure your local SQL Server container (or LocalDB) is running, then apply the Entity Framework Core migrations to build your database tables:
```bash
dotnet ef database update
```
*(You can now run `dotnet run` to launch your project locally!)*

---

##  Daily Development Workflow

When you are ready to write code, follow this exact workflow:

1. **Pull the latest changes from `main`:**
   ```bash
   git checkout main
   git pull origin main
   ```
2. **Create and switch to your feature branch:**
   ```bash
   git checkout -b feature/my-awesome-feature
   ```
3. **Write code, test locally MAKE SURE THE CODE RUNS WITHOUT ANY EXPTIONS OR LOGICAL ERRORS , and commit your work:**
   ```bash
   git add .
   git commit -m "feat: added the awesome feature layout"
   ```
4. **Push your feature branch to GitHub:**
 this will prompt you for the set upstream  when you run it for the first time  read the command and applay it and then the following command again 
   ```bash
   git push -u origin feature/my-awesome-feature
   ```
5. **Open a Pull Request:**
   Go to GitHub, open a PR from `feature/my-awesome-feature` into `main`, request review, and merge it once approved!

---

## 🚀 Running the Project in Development

The project uses **ASP.NET Core MVC (.NET 10)** paired with **Tailwind CSS v4 Standalone CLI** and **Alpine.js**. Razor views automatically hot-reload and Tailwind scans all `.cshtml` and `.js` files for class changes.

### Initial Frontend Setup (First Time Only)
Navigate into the web application folder and install the local Tailwind CLI and dev tools:
```bash
cd depiR5gp
npm install
```
*(Note: Zero CDNs are used. Tailwind CLI, Alpine.js, and the Lucide SVG sprite are 100% self-hosted inside `wwwroot/`)*.

---

### Running Locally (Two Ways)

#### Option 1: Single-Command Concurrent Watch (Recommended)
Run both the .NET server with hot-reload and the Tailwind CSS file watcher simultaneously in a single terminal:
```bash
cd depiR5gp
npm run dev
```
This runs:
- `[dotnet]` → `dotnet watch run` (ASP.NET Core with Hot Reload)
- `[tailwind]` → `@tailwindcss/cli -i ./wwwroot/css/input.css -o ./wwwroot/css/site.css --watch`

Open your browser at:
👉 `http://localhost:5249` or `https://localhost:7101`

---

#### Option 2: Separate Terminals
If you prefer keeping your build logs separate:

**Terminal 1: Start Tailwind CSS Watcher**
```bash
cd depiR5gp
npm run css:watch
```

**Terminal 2: Launch ASP.NET Core MVC**
```bash
cd depiR5gp
dotnet watch run
```

---

### Production Build / CI/CD Deployment
For production builds (Azure App Service / GitHub Actions), compile the minified stylesheet before publishing:
```bash
cd depiR5gp
npm run css:build
dotnet publish -c Release -o ./publish
```

---

## 🧭 Architecture & Navigation Blueprint

- **Client-Side Routing:** The frontend mirrors the prototype's instant routing via hash fragments (`#home`, `#dishes`, `#dish/{id}`, `#chefs`, `#chef/{id}`, `#cart`, `#checkout`, `#orders`, `#profile`, `#loyalty`, `#favorites`, `#inbox`, `#my-cases`, `#admin`, `#support`, `#styleguide`, `#ai-assistant`). All screen partials are located under `Views/*/Partials/` and orchestrated in `Views/Home/Index.cshtml`.
- **State & Localization:** Global state (language, RTL direction, dark mode, role switcher, cart storage, and toasts) is managed through `wwwroot/js/store.js` via Alpine.js (`$store.app`).
- **Icons:** 73 local SVG vector symbols in `_Sprite.cshtml` and `wwwroot/icons/sprite.svg` invoked cleanly via `<svg><use href="#icon-name"></use></svg>`.
- **Backend Data-Binding Notes:** All views contain explicit, granular `<!-- TODO [Backend]: ... -->` markers indicating where to bind Entity Framework Core queries, ASP.NET Core Identity authentication, and SignalR real-time hubs.

---

## 🎯 Implemented UI Features & Views Inventory (Backlog Parity)

All 58 actionable `[UI]` / `[FE]` subtasks audited from `Copy of Home_Chef_Product_Backlog_Specification.md` are fully implemented:

### 1. Chef Operations & Menu Management
| View / Component | Backlog Subtasks | Description |
| :--- | :--- | :--- |
| `_EditChefProfileModal.cshtml` | `SCRUM-30` | Edit kitchen name (EN/AR), bio, photo, pickup area, and weekly working hours schedule. |
| `_AddDishModal.cshtml` | `SCRUM-36`, `SCRUM-61` | Add dish form with name (EN/AR), description, category, price, prep time, availability, and preset option groups builder (radio choose-one vs checkbox choose-many with surcharges). |
| `_EditDishModal.cshtml` | `SCRUM-46`, `SCRUM-61` | Pre-populated modal to edit existing menu dishes and custom option presets. |
| `_DeleteDishConfirmDialog.cshtml` | `SCRUM-51` | Deletion confirmation dialog with active orders policy warning. |
| `_RejectOrderDialog.cshtml` | `SCRUM-81` | Rejection reason selector modal (Kitchen at capacity, missing ingredients, early close). |
| `_ChefDashboard.cshtml` | `SCRUM-96`, `SCRUM-556`, `SCRUM-511` | Order history tab with status filter, customer reviews tab in read-only mode, and financial wallet ledger with 15% commission itemization and instant InstaPay payout trigger. |
| `_PublicChef.cshtml` | `SCRUM-26`, `SCRUM-66` | Working hours schedule table, pickup area privacy disclaimer, and kitchen closed warning banner with disabled cart additions. |

### 2. Customer Order Lifecycle, Reviews & Cart
| View / Component | Backlog Subtasks | Description |
| :--- | :--- | :--- |
| `_CancelOrderDialog.cshtml` | `SCRUM-463` | Customer cancellation modal when order is 'Pending' with 100% full refund guarantee. |
| `_OrderReviewModal.cshtml` | `SCRUM-245`, `SCRUM-548` | 1–5 star interactive rating modal with feedback comments, highlight tags, and anonymous toggle. |
| `_RequestRefundModal.cshtml` | `SCRUM-527`, `SCRUM-539` | Customer refund claim modal with reason category and payout destination (Wallet Credit vs Card Reversal). |
| `_OrderTracking.cshtml` | `SCRUM-230`, `SCRUM-463` | Live timeline, contextual action buttons (Cancel, Rate, Refund, Need Help), preset breakdown, and receipt summary. |
| `_CartPage.cshtml` | `SCRUM-191`, `SCRUM-203-205` | Undo item removal toast, top warning alert banner, line-item unavailable / price updated badges, and disabled checkout validation. |
| `_CustomerProfile.cshtml` | `SCRUM-226`, `SCRUM-249` | Filterable order history with re-order buttons, and dedicated two-tab Favorites section ('Favorite Dishes' and 'Favorite Chefs') with empty states. |
| `_EditCustomerProfileModal.cshtml` | `SCRUM-325` | Edit customer name, phone, email, default address, and dietary preferences. |

### 3. Customer Support & Disputes
| View / Component | Backlog Subtasks | Description |
| :--- | :--- | :--- |
| `_ContactSupportModal.cshtml` | `SCRUM-389` | Ticket submission modal with subject, category, optional order reference, and detailed description. |
| `_MySupportCases.cshtml` | `SCRUM-409`, `SCRUM-397` | Customer-facing ticket list, support agent message thread, follow-up reply box, and closed ticket banner. |
| `_SupportPortal.cshtml` | `SCRUM-406` | Support desk with ticket status transition dropdown (`Open`, `In Progress`, `Resolved`, `Closed`). |

### 4. Platform Oversight & Financial Reconciliation
| View / Component | Backlog Subtasks | Description |
| :--- | :--- | :--- |
| `_CashReconciliationModal.cshtml` | `SCRUM-519` | Reconciles cash-on-pickup collections against the 15% platform commission owed. |
| `_AdminPayoutModal.cshtml` | `SCRUM-523` | Chef payout disbursement modal via InstaPay GPA transfer channel and payout history log. |
| `_OrderSettlementModal.cshtml` | `SCRUM-507`, `SCRUM-515` | Order financial drill-down itemizing Gross GMV, 15% platform commission, and net payable. |
| `_AdminDashboard.cshtml` | `SCRUM-361`, `SCRUM-366`, `SCRUM-552` | User search & role filter with suspension dialog, review moderation workflow (Approve/Reject), and weekly GMV sparkline trend chart. |
| `_AIFaqManagement.cshtml` | `SCRUM-493` | AI Assistant knowledge base FAQ management tool. |

### 5. Multi-Conversation Real-Time Inbox & Auth
| View / Component | Backlog Subtasks | Description |
| :--- | :--- | :--- |
| `_ChatInboxPage.cshtml` | `SCRUM-308`, `SCRUM-294` | Dedicated inbox view (`#inbox`) with conversation switcher, online presence dots, and message bubbles. |
| `_Header.cshtml` | `SCRUM-299` | Live Chat Inbox button with unread count `2` badge and dropdown navigation links. |
| `_ForgotPasswordModal.cshtml` | `SCRUM-320` | Password recovery modal with reset link request, email simulation, and new password confirmation form. |

