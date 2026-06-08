using Microsoft.EntityFrameworkCore;
using PolicyManagement.Domain.Entities;
using PolicyManagement.Infrastructure.Persistence.Configurations;

namespace PolicyManagement.Infrastructure.Persistence;

public class PolicyDbContext : DbContext
{
    public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options) { }

    public DbSet<Policy> Policies => Set<Policy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PolicyConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
