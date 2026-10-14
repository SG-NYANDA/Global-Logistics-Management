using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Data;

public sealed class GlmsDbContext(DbContextOptions<GlmsDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>()
            .HasIndex(client => client.Name);

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasIndex(contract => contract.Reference).IsUnique();
            entity.Property(contract => contract.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(contract => contract.StartDate).HasColumnType("date");
            entity.Property(contract => contract.EndDate).HasColumnType("date");
            entity.HasOne(contract => contract.Client)
                .WithMany(client => client.Contracts)
                .HasForeignKey(contract => contract.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_Contracts_DateRange", "[EndDate] >= [StartDate]"));
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(request => request.UsdCost).HasPrecision(18, 2);
            entity.Property(request => request.ExchangeRate).HasPrecision(18, 6);
            entity.Property(request => request.LocalCost).HasPrecision(18, 2);
            entity.HasOne(request => request.Contract)
                .WithMany(contract => contract.ServiceRequests)
                .HasForeignKey(request => request.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}