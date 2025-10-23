using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using API.Setup;

namespace API.Services;

public interface ICommonService
{
    Task<Result<List<ProductTypeDto>>> GetProductTypesAsync();
}

public class CommonService(ISharedContainer container) : BaseService(container), ICommonService
{
    
    public async Task<Result<List<ProductTypeDto>>> GetProductTypesAsync()
    {
        var productTypes = await Dapper.GetAllAsync<ProductType>();
        var productTypeDtos = productTypes.Select(pt => Mapper.Map<ProductTypeDto>(pt)).ToList();
        return Result<List<ProductTypeDto>>.Success(productTypeDtos);
    }
}