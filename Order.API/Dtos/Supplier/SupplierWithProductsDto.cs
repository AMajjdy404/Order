using Order.API.Dtos.Pagination;

namespace Order.API.Dtos.Supplier
{
    public class SupplierWithProductsDto
    {
        public SupplierDto Supplier { get; set; }
        public PagedResponseDto<SupplierProductDto> SupplierProducts { get; set; }
    }
}
