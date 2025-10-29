using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;

namespace API.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.PrimaryImage, opt => opt.Ignore())
            .ForMember(dest => dest.PriceIcon, opt => opt.Ignore());
        
        CreateMap<Product, ProductDetailDto>()
            .ForMember(dest => dest.PrimaryImage, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.PriceIcon, opt => opt.Ignore())
            .ForMember(dest => dest.Subcategories, opt => opt.Ignore());

        CreateMap<ProductFormDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<ProductType, ProductTypeDto>();
        
        CreateMap<Tag, TagDto>();
    }
}