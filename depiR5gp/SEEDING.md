# Seeding — Quick Guide

Seeders run automatically on app startup, after migrations. Two tiers:

| Tier | What it creates | When it runs |
|------|-----------------|--------------|
| **Core** | Roles, platform settings (12% commission), baseline FAQs, categories & cuisines, optional first admin | Every environment (Dev / Staging / Production) |
| **Demo** | Demo users, chefs, dishes, orders, reviews (`*@seed.homechef.test`, password `Demo@12345`) | **Only** when environment is `Development` **and** `Seeding:EnableDemoData=true` |

> Demo data is hard-blocked outside Development in `Program.cs`, even if the setting is turned on by mistake.

## Change this → get that

| Setting (`appsettings*.json` or env var) | Value | Result |
|---|---|---|
| `Seeding:Enabled` / `Seeding__Enabled` | `false` | No seeding at all |
| `Seeding:EnableDemoData` | `true` (Development only) | Demo users/dishes/orders for local testing |
| `Seeding:EnableDemoData` | `false` | Clean app: Core data only |
| `Seeding:AdminEmail` + `Seeding:AdminPassword` (`Seeding__AdminEmail`, `Seeding__AdminPassword`) | both set | Creates the first Admin account if it doesn't exist (never overwrites) |
| `Seeding:DefaultPassword` / `DemoEmailDomain` | any | Password / email domain of demo accounts |

Defaults: `appsettings.json` → `Enabled=true, EnableDemoData=true`, so any dev who runs the app gets Core + Demo data with no config. This is safe for deployment because `Program.cs` forces demo data **off** in every non-Development environment (Azure runs as `Production`).

> Seeding runs when the **app starts** (after migrations are applied), not during `dotnet ef database update`. To get the latest seed data, pull and run `dotnet run`; seeders are idempotent and only add what's missing.

## Local development
```bash
dotnet run          # Development env → Core + Demo seed
```
Logins: `admin@seed.homechef.test`, `chef.layla@seed.homechef.test`, `customer.sara@seed.homechef.test` — password `Demo@12345`.

## Reset local demo data
Drop the DB (`dotnet ef database drop`) and rerun; seeders are idempotent (re-running never duplicates).

## Deployment (`.github/workflows/deploy.yml` → Azure Web App, push to `deploy` branch)
The app runs as `Production`, so on first boot it: applies migrations → creates roles → seeds Core data → **no demo data**.

One-time setup in Azure → Web App → Configuration → Application settings:
```
ConnectionStrings__DefaultConnection = <azure sql connection string>
Seeding__AdminEmail                  = admin@yourdomain.com
Seeding__AdminPassword               = <strong password>
```
Do **not** set `ASPNETCORE_ENVIRONMENT=Development` in Azure. After first login, rotate the admin password and remove `Seeding__AdminPassword`.
