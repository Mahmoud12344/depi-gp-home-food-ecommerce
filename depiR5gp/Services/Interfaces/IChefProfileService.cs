using depiR5gp.Models.Chefs;

namespace depiR5gp.Services;

public interface IChefProfileService
{
    Task<ChefProfileViewModel?> GetChefProfileAsync(int userId);
}