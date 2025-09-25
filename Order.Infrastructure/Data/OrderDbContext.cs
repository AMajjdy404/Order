using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Models;

namespace Order.Infrastructure.Data
{
    public class OrderDbContext: IdentityDbContext<AppOwner>
    {
        public DbSet<AppOwner> AppOwners { get; set; }   
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }
        public DbSet<Product> Products { get; set; }
 
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<BuyerOrder> BuyerOrders { get; set; }
        public DbSet<SupplierOrder> SupplierOrders { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }
        public DbSet<MyOrder> MyOrders { get; set; }
        public DbSet<SupplierOrderItem> SupplierOrderItems { get; set; }
        public DbSet<SupplierPenalty> SupplierPenalties { get; set; }
        public DbSet<ReturnOrderItem> ReturnOrders { get; set; }
        public DbSet<SupplierStatement> SupplierStatements { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<SupplierDeliveryStation> SupplierDeliveryStations { get; set; }
        public DbSet<DeliveryStation> DeliveryStations { get; set; }


        public OrderDbContext(DbContextOptions<OrderDbContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {


            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
