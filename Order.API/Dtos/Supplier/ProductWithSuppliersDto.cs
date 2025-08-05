using Order.API.Dtos.Buyer;

namespace Order.API.Dtos.Supplier
{
    public class ProductWithSuppliersDto
    {
        public ProductDto Product { get; set; }
        public List<SupplierWithProductDto> Suppliers { get; set; }
    }
}
