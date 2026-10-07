using ApiTechStore.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiTechStore.Data;

public sealed class TechStoreDbContext(DbContextOptions<TechStoreDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechStoreDbContext).Assembly);
    }
}
