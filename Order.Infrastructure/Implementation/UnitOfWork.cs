using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order.Domain.Interfaces;
using Order.Domain.Models;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;

        public IGenericRepository<Buyer> Buyers { get; }
        public IGenericRepository<Supplier> Suppliers { get; }
        public IGenericRepository<SupplierProduct> SupplierProducts { get; }
        public IGenericRepository<Product> Products { get; }
        public IGenericRepository<BuyerOrder> BuyerOrders { get; }
        public IGenericRepository<SupplierOrder> SupplierOrders { get; }
        public IGenericRepository<OrderItem> OrderItems { get; }
        public IGenericRepository<MyOrder> MyOrders { get; }

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
            Buyers = new GenericRepository<Buyer>(_context);
            Suppliers = new GenericRepository<Supplier>(_context);
            SupplierProducts = new GenericRepository<SupplierProduct>(_context);
            Products = new GenericRepository<Product>(_context);
            BuyerOrders = new GenericRepository<BuyerOrder>(_context);
            SupplierOrders = new GenericRepository<SupplierOrder>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);
            MyOrders = new GenericRepository<MyOrder>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
