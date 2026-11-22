using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds realistic demo users (Admin, Support, Chefs, Customers) with mixed account statuses
/// (Active, Suspended, Deleted) and audit history to allow manual and automated filter/pagination testing.
/// </summary>
public class DemoUsersSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Demo;
    public int Order => 10;

    private sealed record DemoUserData(
        string EmailPrefix,
        string FirstName,
        string LastName,
        string Role,
        UserStatus Status,
        int PointsBalance,
        string? Address,
        string? PhoneNumber,
        string? KitchenName,
        string? KitchenBio,
        string? StatusChangeReason,
        int StatusDaysAgo = 0,
        string? KitchenStatus = null,
        bool IncludeLicenseDoc = true);

    public async Task SeedAsync(SeedContext context)
    {
        var domain = context.Options.DemoEmailDomain.TrimStart('@');
        var password = context.Options.DefaultPassword;

        var demoUsers = new List<DemoUserData>
        {
            // System Staff
            new("admin", "Admin", "User", AppRoles.Admin, UserStatus.Active, 0, "Platform HQ, Cairo", "01000000001", null, null, null),
            new("support", "Support", "Specialist", AppRoles.Support, UserStatus.Active, 0, "Platform Operations, Giza", "01000000002", null, null, null),

            // Chefs - Diverse statuses and profiles
            new("chef.hassan", "Hassan", "Mostafa", AppRoles.Chef, UserStatus.Active, 50,
                "14 El-Bahr St, Dokki, Giza", "01011110001",
                "Hassan's Traditional Grill", "Authentic Egyptian grilled meats, kofta, and homestyle tajines.", null),

            new("chef.layla", "Layla", "Mahmoud", AppRoles.Chef, UserStatus.Active, 120,
                "28 Cleopatra St, Heliopolis, Cairo", "01011110002",
                "Layla's Artisanal Patisserie", "Freshly baked French-Egyptian pastries, kunafa rolls, and cakes.", null),

            new("chef.tarek", "Tarek", "Salem", AppRoles.Chef, UserStatus.Active, 30,
                "5 Corniche El-Nil, Maadi, Cairo", "01011110003",
                "Tarek's Alexandria Seafood", "Fresh Mediterranean fish, sayadeya rice, and spiced seafood platters.", null),

            new("chef.mona", "Mona", "Ezzat", AppRoles.Chef, UserStatus.Active, 200,
                "8 Road 9, Maadi, Cairo", "01011110004",
                "Mona's Vegan Healthy Kitchen", "Nutritious plant-based Middle Eastern bowls and vegan koshari.", null),

            new("chef.karim", "Karim", "Farouk", AppRoles.Chef, UserStatus.Suspended, 10,
                "22 Abbasia Square, Cairo", "01011110005",
                "Karim's Slow-Cooked Smoked Meats", "Smoked brisket, ribs, and homemade BBQ sides.",
                "Multiple hygiene reports flagged for verification during routine platform audit.", 7),

            new("chef.nour", "Nour", "El-Din", AppRoles.Chef, UserStatus.Suspended, 0,
                "33 Ramses St, Downtown, Cairo", "01011110006",
                "Nour's Oriental Delights", "Traditional Egyptian desserts and sweets.",
                "High rate of unfulfilled orders without prior notice to customers.", 14),

            new("chef.samir", "Samir", "Adel", AppRoles.Chef, UserStatus.Deleted, 0,
                "11 Sphinx St, Giza", "01011110007",
                "Samir's Gourmet Burgers", "Hand-crafted beef burgers and loaded waffle fries.",
                "Permanent account deactivation requested by chef owner.", 30),

            // Chefs with Pending Applications (for Admin Review & Chef Application Status testing)
            new("chef.amina", "Amina", "El-Gohary", AppRoles.Chef, UserStatus.Active, 0,
                "19 Road 206, Degla, Maadi, Cairo", "01011110008",
                "Amina's Heritage Kitchen", "Traditional duck molokhia, stuffed pigeon, and authentic rural Egyptian casseroles.",
                null, 0, "Pending", true),

            new("chef.hany", "Hany", "Roshdy", AppRoles.Chef, UserStatus.Active, 0,
                "42 El-Merghany St, Heliopolis, Cairo", "01011110009",
                "Hany's Gourmet Grills", "Charcoal-grilled kebabs, tarb, and slow-braised oxtail tagines.",
                null, 0, "Pending", true),

            new("chef.nabil", "Nabil", "Fawzy", AppRoles.Chef, UserStatus.Active, 0,
                "12 Abbas El-Akkad St, Nasr City, Cairo", "01011110010",
                "Om Nabil Casseroles", "Clay-oven okra with lamb, moussaka, and classic homemade béchamel pasta.",
                null, 0, "Pending", false),

            new("chef.salma", "Salma", "Hegazy", AppRoles.Chef, UserStatus.Active, 0,
                "6 Shooting Club St, Dokki, Giza", "01011110011",
                "Salma's Upper Egyptian Table", "Authentic Saidi keshk, fayesh, and slow-cooked earthen pot specialties.",
                null, 0, "Pending", true),

            // Customers - Diverse statuses, points, and locations
            new("customer.sara", "Sara", "Ahmed", AppRoles.Customer, UserStatus.Active, 250,
                "12 El-Gezira St, Zamalek, Cairo", "01022220001", null, null, null),

            new("customer.ahmed", "Ahmed", "Mansour", AppRoles.Customer, UserStatus.Active, 80,
                "44 Degla St, Maadi, Cairo", "01022220002", null, null, null),

            new("customer.mariam", "Mariam", "Khalil", AppRoles.Customer, UserStatus.Active, 500,
                "15 Cleopatra St, Heliopolis, Cairo", "01022220003", null, null, null),

            new("customer.omar", "Omar", "Nasser", AppRoles.Customer, UserStatus.Active, 0,
                "7 Abbasia Square, Cairo", "01022220004", null, null, null),

            new("customer.fatma", "Fatma", "Zahraa", AppRoles.Customer, UserStatus.Active, 120,
                "22 Corniche El Nile, Giza", "01022220005", null, null, null),

            new("customer.youssef", "Youssef", "Shawky", AppRoles.Customer, UserStatus.Suspended, 0,
                "9 Ramses St, Downtown, Cairo", "01022220006", null, null,
                "Chargeback abuse and repeated suspicious card payment transactions.", 5),

            new("customer.hoda", "Hoda", "Fawzy", AppRoles.Customer, UserStatus.Suspended, 15,
                "3 Abbas El Akkad, Nasr City, Cairo", "01022220007", null, null,
                "Reported abusive conduct towards platform delivery personnel.", 10),

            new("customer.khaled", "Khaled", "Ibrahim", AppRoles.Customer, UserStatus.Deleted, 0,
                "5 Dokki St, Giza", "01022220008", null, null,
                "User requested GDPR account deletion.", 21)
        };

        // First find or ensure admin account exists so we can use its ID for status changes
        AppUser? adminUser = await context.UserManager.FindByEmailAsync($"admin@{domain}");

        foreach (var data in demoUsers)
        {
            var email = $"{data.EmailPrefix}@{domain}";
            var existingUser = await context.UserManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                if (data.Role == AppRoles.Admin && adminUser == null)
                {
                    adminUser = existingUser;
                }
                continue;
            }

            var newUser = new AppUser
            {
                UserName = email,
                Email = email,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Address = data.Address,
                PhoneNumber = data.PhoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Status = data.Status,
                PointsBalance = data.PointsBalance
            };

            var createResult = await context.UserManager.CreateAsync(newUser, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                context.Logger.LogWarning("Failed to create demo user {Email}: {Errors}", email, errors);
                continue;
            }

            var roleResult = await context.UserManager.AddToRoleAsync(newUser, data.Role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                context.Logger.LogWarning("Failed to assign role {Role} to user {Email}: {Errors}", data.Role, email, errors);
            }

            if (data.Role == AppRoles.Admin && adminUser == null)
            {
                adminUser = newUser;
            }

            // If Chef, ensure ChefProfile exists
            if (data.Role == AppRoles.Chef)
            {
                var hasProfile = await context.DbContext.ChefProfiles.AnyAsync(c => c.UserId == newUser.Id);
                if (!hasProfile)
                {
                    var resolvedKitchenStatus = data.KitchenStatus
                        ?? (data.Status == UserStatus.Active
                            ? "Approved"
                            : (data.Status == UserStatus.Suspended ? "Suspended" : "Rejected"));

                    var chefProfile = new ChefProfile
                    {
                        UserId = newUser.Id,
                        KitchenName = data.KitchenName ?? $"{data.FirstName}'s Kitchen",
                        Bio = data.KitchenBio ?? "Home-cooked food made with love.",
                        Address = data.Address,
                        Status = resolvedKitchenStatus,
                        HasAcceptedTerms = true,
                        CommissionRate = 0.15m
                    };

                    chefProfile.Documents.Add(new ChefDocument
                    {
                        DocType = "Identity",
                        FileUrl = "/uploads/chef_documents/1380f947-8228-4f68-bcef-458dcf92bf9d_Identity.pdf"
                    });
                    chefProfile.Documents.Add(new ChefDocument
                    {
                        DocType = "HealthCertificate",
                        FileUrl = "/uploads/chef_documents/324da830-0875-4d7e-9c85-ac960e8533be_HealthCertificate.pdf"
                    });
                    if (data.IncludeLicenseDoc)
                    {
                        chefProfile.Documents.Add(new ChefDocument
                        {
                            DocType = "CulinaryLicense",
                            FileUrl = "/uploads/chef_documents/8c19c441-4293-468e-b01c-c3d774441548_CulinaryLicense.pdf"
                        });
                    }

                    context.DbContext.ChefProfiles.Add(chefProfile);
                }
            }

            // If Suspended or Deleted, record status history entry for audit trail
            if (data.Status != UserStatus.Active && !string.IsNullOrWhiteSpace(data.StatusChangeReason))
            {
                var historyEntry = new UserStatusHistory
                {
                    UserId = newUser.Id,
                    ChangedByUserId = adminUser?.Id,
                    Status = data.Status,
                    Reason = data.StatusChangeReason,
                    ChangedAt = DateTime.UtcNow.AddDays(-data.StatusDaysAgo)
                };
                context.DbContext.UserStatusHistories.Add(historyEntry);
            }

            context.Logger.LogInformation("Seeded demo user: {Email} (Role: {Role}, Status: {Status})", email, data.Role, data.Status);
        }

        await context.DbContext.SaveChangesAsync();
    }
}

