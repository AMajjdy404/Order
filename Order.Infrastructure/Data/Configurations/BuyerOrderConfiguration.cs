using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Models;

namespace Order.Infrastructure.Data.Configurations
{
    public class BuyerOrderConfiguration : IEntityTypeConfiguration<BuyerOrder>
    {
        public void Configure(EntityTypeBuilder<BuyerOrder> builder)
        {
            builder.HasKey(bo => bo.Id);
            builder.HasMany(bo => bo.OrderItems)
                   .WithOne(oi => oi.BuyerOrder)
                   .HasForeignKey(oi => oi.BuyerOrderId);
        }
    }
}
