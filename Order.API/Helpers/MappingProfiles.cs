using AutoMapper;
using Order.API.Dtos.Buyer;
using Order.API.Dtos.Dashboard;
using Order.API.Dtos.Supplier;
using Order.Domain.Models;

namespace Order.API.Helpers
{
    public class MappingProfiles:Profile
    {
        public MappingProfiles()
        {
            //CreateMap<Product, ProductToReturnDto>()
            //    .ForMember(d => d.Company, o => o.MapFrom(s => s.Company.Name))
            //    .ForMember(d => d.Section, o => o.MapFrom(s => s.Section.Name));

            CreateMap<RegisterBuyerDto, Buyer>()
            .ForMember(d => d.PropertyInsideImagePath, o => o.Ignore())
            .ForMember(d => d.PropertyOutsideImagePath, o => o.Ignore())
            .ForMember(d => d.Password, o => o.Ignore());

            CreateMap<Buyer, BuyerToReturnDto>()
                .ForMember(d => d.PropertyInsideImagePath, o => o.MapFrom<BuyerInsidePictureUrlResolver>())
                .ForMember(d => d.PropertyOutsideImagePath, o => o.MapFrom<BuyerOutsidePictureUrlResolver>());

            CreateMap<RegisterSupplierDto, Supplier>()
           .ForMember(d => d.WarehouseImageUrl, o => o.Ignore())
           .ForMember(d => d.Password, o => o.Ignore())
           .ForMember(d => d.ProfitPercentage, o => o.Ignore())
           .ForMember(dest => dest.SupplierType, opt => opt.MapFrom(src => Enum.Parse<SupplierType>(src.SupplierType)))
           .ForMember(dest => dest.MinimumOrderPrice, opt => opt.MapFrom(src => src.MinimumOrderPrice))
           .ForMember(dest => dest.MinimumOrderItems, opt => opt.MapFrom(src => src.MinimumOrderItems));
            

            CreateMap<Supplier, SupplierToReturnDto>()
                .ForMember(d => d.WarehouseImageUrl, o => o.MapFrom<SupplierWarehousePictureUrlResolver>())
                .ForMember(dest => dest.SupplierType, opt => opt.MapFrom(src => src.SupplierType.ToString()))
                    .ForMember(dest => dest.MinimumOrderPrice, opt => opt.MapFrom(src => src.MinimumOrderPrice))
                    .ForMember(dest => dest.MinimumOrderItems, opt => opt.MapFrom(src => src.MinimumOrderItems));

            CreateMap<PagedResult<Buyer>, PagedResult<BuyerToReturnDto>>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

            CreateMap<Product, ProductDto>();

            CreateMap<SupplierProduct, SupplierProductDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Supplier, SupplierDto>();
           

        }
    }
}
