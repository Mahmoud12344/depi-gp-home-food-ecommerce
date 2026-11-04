using depiR5gp.Data;
using depiR5gp.Models.Platform;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class PlatformSettingRepository(ApplicationDbContext context) : GenericRepository<PlatformSettings>(context), IPlatformSettingRepository {
    
}