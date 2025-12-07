using API.Models.DsfTables;
using API.Models.Dtos;
using AutoMapper;

namespace API.Mappings;

public class OrderMappingProfiles : Profile
{
    public OrderMappingProfiles()
    {
        CreateMap<Order, OrderDetailDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
        CreateMap<OrderItem, OrderItemDto>();
    }
}