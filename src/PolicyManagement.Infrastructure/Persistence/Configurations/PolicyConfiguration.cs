using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyManagement.Domain.Entities;

namespace PolicyManagement.Infrastructure.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PolicyNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PolicyholderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LineOfBusiness)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.PremiumAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.EffectiveDate)
            .IsRequired();

        builder.Property(x => x.ExpiryDate)
            .IsRequired();

        builder.Property(x => x.Region)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Underwriter)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FlaggedForReview)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.PolicyNumber).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Region);
        builder.HasIndex(x => x.LineOfBusiness);
        builder.HasIndex(x => new { x.EffectiveDate, x.ExpiryDate });
    }
}
