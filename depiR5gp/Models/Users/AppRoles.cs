namespace depiR5gp.Models.Users;

public static class AppRoles
{
    public const string Customer = "Customer";
    public const string Chef = "Chef";
    public const string Admin = "Admin";
    public const string Support = "Support";

    public static readonly string[] All = [Customer, Chef, Admin, Support];
}
