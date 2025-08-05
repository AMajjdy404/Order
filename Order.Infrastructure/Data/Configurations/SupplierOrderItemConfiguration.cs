using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Models;

namespace Order.Infrastructure.Data.Configurations
{
    public class SupplierOrderItemConfiguration : IEntityTypeConfiguration<SupplierOrderItem>
    {
        public void Configure(EntityTypeBuilder<SupplierOrderItem> builder)
        {
            builder.HasKey(soi => soi.Id);

            builder.HasOne(soi => soi.SupplierOrder)
                   .WithMany(so => so.Items)
                   .HasForeignKey(soi => soi.SupplierOrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(soi => soi.SupplierProduct)
                   .WithMany()
                   .HasForeignKey(soi => soi.SupplierProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }


}
