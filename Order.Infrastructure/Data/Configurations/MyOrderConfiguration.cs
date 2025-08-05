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
    public class MyOrderConfiguration : IEntityTypeConfiguration<MyOrder>
    {
        public void Configure(EntityTypeBuilder<MyOrder> builder)
        {
            builder.HasKey(mo => mo.Id);

            builder.HasOne<BuyerOrder>()
                   .WithMany()
                   .HasForeignKey(mo => mo.BuyerOrderId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<SupplierProduct>()
                   .WithMany()
                   .HasForeignKey(mo => mo.SupplierProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
