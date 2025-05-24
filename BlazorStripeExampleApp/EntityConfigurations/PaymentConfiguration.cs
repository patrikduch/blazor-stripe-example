namespace BlazorStripeExample.EntityConfigurations;

using BlazorStripeExample.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SessionId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.CustomerEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.AmountTotal)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();
    }
}
