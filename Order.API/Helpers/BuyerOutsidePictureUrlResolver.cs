using AutoMapper;
using Order.API.Dtos.Buyer;
using Order.Domain.Models;

namespace Order.API.Helpers
{
    public class BuyerOutsidePictureUrlResolver : IValueResolver<Buyer, BuyerToReturnDto, string>
    {
        private readonly IConfiguration _configuration;

        public BuyerOutsidePictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Buyer source, BuyerToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PropertyOutsideImagePath))
            {
                return $"{_configuration["BaseApiUrl"]}{source.PropertyOutsideImagePath}";
            }
            return string.Empty;
        }
    }
}
