using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Domain.Models
{
    public class MyOrder
    {
        public int Id { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public DateOnly OrderDate { get; set; }
        public int? BuyerOrderId { get; set; }
        public int BuyerId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        public List<MyOrderItem> Items { get; set; } = new List<MyOrderItem>();
    }

}
