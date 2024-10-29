using EFCoreVsDapper.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreVsDapper;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TestEcomDB;Trusted_Connection=True;");
    }

}
