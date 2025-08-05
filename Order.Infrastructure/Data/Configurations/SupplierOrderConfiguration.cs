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
    public class SupplierOrderConfiguration : IEntityTypeConfiguration<SupplierOrder>
    {
        public void Configure(EntityTypeBuilder<SupplierOrder> builder)
        {
            builder.HasKey(so => so.Id);

            builder.HasOne(so => so.Supplier)
                   .WithMany()
                   .HasForeignKey(so => so.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(so => so.Items)
                   .WithOne(oi => oi.SupplierOrder)
                   .HasForeignKey(oi => oi.SupplierOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }





}
