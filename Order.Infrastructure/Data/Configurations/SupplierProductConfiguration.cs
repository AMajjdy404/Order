using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Models;

namespace Order.Infrastructure.Data.Configurations
{
    public class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
    {
        public void Configure(EntityTypeBuilder<SupplierProduct> builder)
        {
            builder.HasKey(sp => sp.Id);
            builder.HasOne(sp => sp.Product)
                   .WithMany()
                   .HasForeignKey(sp => sp.ProductId);
            builder.HasOne(sp => sp.Supplier)
                   .WithMany()
                   .HasForeignKey(sp => sp.SupplierId);
        }
    }
}
