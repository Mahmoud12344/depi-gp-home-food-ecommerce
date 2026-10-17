using depiR5gp.Models;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options){
    public DbSet<TestModel> TestModels{ get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<TestModel>().HasData(
            new TestModel(){Id=1,Name = "first test"},
            new TestModel(){Id=2,Name = "second test"}
            );



    }
}
