using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order.Domain.Models;

namespace Order.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Buyer> Buyers { get; }
        IGenericRepository<Supplier> Suppliers { get; }
        IGenericRepository<SupplierProduct> SupplierProducts { get; }
        IGenericRepository<Product> Products { get; }
        IGenericRepository<BuyerOrder> BuyerOrders { get; }
        IGenericRepository<SupplierOrder> SupplierOrders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<MyOrder> MyOrders { get; }

        Task<int> SaveChangesAsync();
    }

}
