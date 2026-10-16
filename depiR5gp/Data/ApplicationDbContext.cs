using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options){    
}