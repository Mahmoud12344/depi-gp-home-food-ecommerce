# Developer Guide: Seeded Test Accounts & Demo Data

This reference documents the demo accounts automatically seeded in the **Development** environment to assist developers and QA engineers in testing multi-role features, order lifecycles, and moderation workflows.

---

## 🔑 Quick Credentials Reference

All seeded accounts share the same default password:
- **Default Password**: `Demo@12345`
- **Domain**: `@seed.homechef.test`

---

## 🛡️ Platform Administration & Staff Accounts

Use these accounts to test administrative features, review moderation dashboards, and user management (`/Admin/Users`).

| Name | Email | Role | Status | Description / Access |
| :--- | :--- | :--- | :--- | :--- |
| **Admin User** | `admin@seed.homechef.test` | `Admin` | `Active` | Full administrative access to the platform dashboard, chef applications, and user moderation. |
| **Support Specialist** | `support@seed.homechef.test` | `Support` | `Active` | Customer support agent for inquiry triage, order dispute mediation, and support cases. |

---

## 👨‍🍳 Chef Accounts & Kitchen Profiles

These accounts represent home chefs with domestic kitchen profiles, various statuses (Active, Suspended, Deactivated), and kitchen specialties.

| Chef Name | Email | Status | Kitchen Name | Cuisine / Bio | Location | Moderation Audit Note |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Hassan Mostafa** | `chef.hassan@seed.homechef.test` | `Active` | Hassan's Traditional Grill | Authentic Egyptian grilled meats, kofta, and homestyle tajines. | Dokki, Giza | — |
| **Layla Mahmoud** | `chef.layla@seed.homechef.test` | `Active` | Layla's Artisanal Patisserie | Freshly baked French-Egyptian pastries, kunafa rolls, and cakes. | Heliopolis, Cairo | — |
| **Tarek Salem** | `chef.tarek@seed.homechef.test` | `Active` | Tarek's Alexandria Seafood | Fresh Mediterranean fish, sayadeya rice, and spiced seafood platters. | Maadi, Cairo | — |
| **Mona Ezzat** | `chef.mona@seed.homechef.test` | `Active` | Mona's Vegan Healthy Kitchen | Nutritious plant-based Middle Eastern bowls and vegan koshari. | Maadi, Cairo | — |
| **Karim Farouk** | `chef.karim@seed.homechef.test` | `Suspended` | Karim's Slow-Cooked Smoked Meats | Smoked brisket, ribs, and homemade BBQ sides. | Abbasia, Cairo | *Multiple hygiene reports flagged for verification during routine platform audit.* |
| **Nour El-Din** | `chef.nour@seed.homechef.test` | `Suspended` | Nour's Oriental Delights | Traditional Egyptian desserts and sweets. | Downtown, Cairo | *High rate of unfulfilled orders without prior notice to customers.* |
| **Samir Adel** | `chef.samir@seed.homechef.test` | `Deleted` | Samir's Gourmet Burgers | Hand-crafted beef burgers and loaded waffle fries. | Sphinx St, Giza | *Permanent account deactivation requested by chef owner.* |

---

## 🛒 Customer Accounts

These accounts represent customers across various neighborhoods in Cairo and Giza with varying loyalty point balances and account states.

| Customer Name | Email | Status | Loyalty Points | Address | Moderation Audit Note |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Sara Ahmed** | `customer.sara@seed.homechef.test` | `Active` | **250** | 12 El-Gezira St, Zamalek, Cairo | — |
| **Ahmed Mansour** | `customer.ahmed@seed.homechef.test` | `Active` | **80** | 44 Degla St, Maadi, Cairo | — |
| **Mariam Khalil** | `customer.mariam@seed.homechef.test` | `Active` | **500** | 15 Cleopatra St, Heliopolis, Cairo | — |
| **Omar Nasser** | `customer.omar@seed.homechef.test` | `Active` | **0** | 7 Abbasia Square, Cairo | — |
| **Fatma Zahraa** | `customer.fatma@seed.homechef.test` | `Active` | **120** | 22 Corniche El Nile, Giza | — |
| **Youssef Shawky** | `customer.youssef@seed.homechef.test` | `Suspended` | **0** | 9 Ramses St, Downtown, Cairo | *Chargeback abuse and repeated suspicious card payment transactions.* |
| **Hoda Fawzy** | `customer.hoda@seed.homechef.test` | `Suspended` | **15** | 3 Abbas El Akkad, Nasr City, Cairo | *Reported abusive conduct towards platform delivery personnel.* |
| **Khaled Ibrahim** | `customer.khaled@seed.homechef.test` | `Deleted` | **0** | 5 Dokki St, Giza | *User requested GDPR account deletion.* |

---

## ⚙️ Configuration & Environment Safeguards

Seeding is **isolated strictly to local development** and will never run in staging, production, or automated deployment branches.

### Toggling Demo Seeding
To disable demo data seeding locally, update your `appsettings.Development.json`:
```json
{
  "Seeding": {
    "Enabled": true,
    "EnableDemoData": false
  }
}
```

### Safety Guarantees:
1. **Idempotency**: Running `dotnet run` multiple times will not duplicate accounts. Existing accounts are detected via email before creation.
2. **Environment Guards**: Seeders are registered only when `builder.Environment.IsDevelopment()` is true, and executed only when `app.Environment.IsDevelopment()` is true.
3. **Production Defaults**: The base `appsettings.json` explicitly sets `"Seeding": { "Enabled": false, "EnableDemoData": false }`.
