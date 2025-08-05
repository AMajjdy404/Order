using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Canceled
    }
    public class SupplierOrder
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public string PaymentMethod { get; set; }
        public OrderStatus Status { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public string PropertyLocation { get; set; }
        public Supplier Supplier { get; set; }
        public List<SupplierOrderItem> Items { get; set; }
    }



}
