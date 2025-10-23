using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;

namespace API.Mappings;

public class CategoryMappingProfiles : Profile
{
    public CategoryMappingProfiles()
    {
        CreateMap<Subcategory, SubcategoryDto>();
    }
}