using AutoMapper;
using Order.API.Dtos.Buyer;
using Order.Domain.Models;

namespace Order.API.Helpers
{
    public class BuyerInsidePictureUrlResolver : IValueResolver<Buyer, BuyerToReturnDto, string>
    {
        private readonly IConfiguration _configuration;

        public BuyerInsidePictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(Buyer source, BuyerToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PropertyInsideImagePath))
            {
                return $"{_configuration["BaseApiUrl"]}{source.PropertyInsideImagePath}";
            }
            return string.Empty;
        }
    }
}
