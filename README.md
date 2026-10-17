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