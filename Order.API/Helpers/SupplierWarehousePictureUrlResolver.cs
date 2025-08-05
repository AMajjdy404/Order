using AutoMapper;
using Order.API.Dtos.Supplier;
using Order.Domain.Models;

namespace Order.API.Helpers
{
    public class SupplierWarehousePictureUrlResolver : IValueResolver<Supplier, SupplierToReturnDto, string>
    {
        private readonly IConfiguration _configuration;

        public SupplierWarehousePictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Supplier source, SupplierToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.WarehouseImageUrl))
            {
                return $"{_configuration["BaseApiUrl"]}{source.WarehouseImageUrl}";
            }
            return string.Empty;
        }
    }
}
