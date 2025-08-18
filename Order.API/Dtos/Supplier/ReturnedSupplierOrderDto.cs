using Order.Domain.Models;

namespace Order.API.Dtos.Supplier
{
    public class ReturnedSupplierOrderDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal WalletPaymentAmount { get; set; }

        // بيانات المورد
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierType { get; set; }

        // بيانات المشتري
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public string PropertyLocation { get; set; }

        // المنتجات
        public List<SupplierOrderItemDto> Items { get; set; }
    }
}
