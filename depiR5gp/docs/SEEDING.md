# Database Seeding Guide

This project includes a tiered, modular, and idempotent database seeding framework.

## Architecture

The seeding pipeline is located in `depiR5gp/Data/Seeding/`.

```
depiR5gp/Data/Seeding/
├── SeedTier.cs             # Tier hierarchy: Core (1) -> Catalog (2) -> Demo (3)
├── ISeeder.cs              # Contract for seeders
├── SeedContext.cs          # Context passed to each seeder (DbContext, UserManager, etc.)
├── SeedingOptions.cs       # Configuration options bound from appsettings
├── DatabaseSeeder.cs       # Sequential orchestrator
├── SeedingExtensions.cs    # DI and WebApplication startup extensions
└── Seeders/
    ├── RolesSeeder.cs      # Core: Seeds Identity roles (Admin, Chef, Customer, Support)
    └── DemoUsersSeeder.cs  # Demo: Seeds 16 accounts (diverse roles, statuses, and history)
```

### Seeding Tiers

Seeders are organized into 3 tiers to guarantee dependency integrity:
1. **`SeedTier.Core`**: Critical system infrastructure (e.g. Identity Roles, SuperAdmin accounts, System Configurations). Always executes when seeding is enabled.
2. **`SeedTier.Catalog`**: Reference and lookup data (e.g. Food Categories, Cuisines, Cities/Governorates).
3. **`SeedTier.Demo`**: Realistic sample records for local development and QA testing (e.g. Chefs with dishes, Customers with orders and carts). Skipped in production or when `EnableDemoData` is `false`.

---

## Configuration

In `appsettings.Development.json` (or environment variables):

```json
{
  "Seeding": {
    "Enabled": true,
    "EnableDemoData": true,
    "DefaultPassword": "Demo@12345",
    "DemoEmailDomain": "seed.homechef.test"
  }
}
```

- **`Enabled`** (`bool`): Master toggle for the entire seeding pipeline.
- **`EnableDemoData`** (`bool`): When `false`, only `Core` and `Catalog` seeders will run.
- **`DefaultPassword`** (`string`): The default password used for generated demo accounts.
- **`DemoEmailDomain`** (`string`): Email domain for demo users (e.g., `chef.layla@seed.homechef.test`).

---

## Pre-seeded Demo Accounts

When `EnableDemoData` is `true`, the following accounts are seeded with password **`Demo@12345`**:

| Email | Role | Status | Notes |
| :--- | :--- | :--- | :--- |
| `admin@seed.homechef.test` | Admin | Active | Full administrative access |
| `support@seed.homechef.test` | Support | Active | Customer support specialist |
| `chef.hassan@seed.homechef.test` | Chef | Active | Kitchen: *Hassan's Traditional Grill* |
| `chef.layla@seed.homechef.test` | Chef | Active | Kitchen: *Layla's Artisanal Patisserie* |
| `chef.tarek@seed.homechef.test` | Chef | Active | Kitchen: *Tarek's Alexandria Seafood* |
| `chef.mona@seed.homechef.test` | Chef | Active | Kitchen: *Mona's Vegan Healthy Kitchen* |
| `chef.karim@seed.homechef.test` | Chef | Suspended | Kitchen: *Karim's Slow-Cooked Smoked Meats* (Hygiene report under audit) |
| `chef.nour@seed.homechef.test` | Chef | Suspended | Kitchen: *Nour's Oriental Delights* (Unfulfilled orders) |
| `chef.samir@seed.homechef.test` | Chef | Deleted | Kitchen: *Samir's Gourmet Burgers* (Voluntary closure) |
| `customer.sara@seed.homechef.test` | Customer | Active | 250 points, Zamalek |
| `customer.ahmed@seed.homechef.test` | Customer | Active | 80 points, Maadi |
| `customer.mariam@seed.homechef.test` | Customer | Active | 500 points, Heliopolis |
| `customer.omar@seed.homechef.test` | Customer | Active | 0 points, Abbasia |
| `customer.fatma@seed.homechef.test` | Customer | Active | 120 points, Giza |
| `customer.youssef@seed.homechef.test` | Customer | Suspended | Suspicious payment/chargebacks |
| `customer.hoda@seed.homechef.test` | Customer | Suspended | Abusive conduct reported by courier |
| `customer.khaled@seed.homechef.test` | Customer | Deleted | Account deletion requested |

---

## How to Add a New Seeder

Adding a new seeder takes just 1 step: create a class in `depiR5gp/Data/Seeding/Seeders/` implementing `ISeeder`. It is automatically discovered and registered via assembly scanning.

### Example: Seeding Categories and Cuisines

```csharp
using depiR5gp.Data.Seeding;
using depiR5gp.Models.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

public class MenuCatalogSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Catalog;
    public int Order => 1;

    public async Task SeedAsync(SeedContext context)
    {
        if (await context.DbContext.Categories.AnyAsync(context.CancellationToken))
        {
            return; // Idempotent check: already seeded
        }

        var categories = new[]
        {
            new Category { Name = "Main Dishes", Description = "Hearty homestyle meals" },
            new Category { Name = "Bakery & Sweets", Description = "Freshly baked pastries and oriental sweets" }
        };

        context.DbContext.Categories.AddRange(categories);
        await context.DbContext.SaveChangesAsync(context.CancellationToken);

        context.Logger.LogInformation("Seeded {Count} categories.", categories.Length);
    }
}
```

### Best Practices for Seeders
1. **Idempotency**: Always verify if the data or key identifier already exists before adding (`AnyAsync`, `FindByEmailAsync`, etc.). Running the seeder multiple times should not cause primary key violations or duplicate records.
2. **Cancellation Token**: Pass `context.CancellationToken` to async EF Core methods.
3. **Respect Tiers**: If entity B depends on entity A, ensure A's seeder has a lower tier or a lower order.

