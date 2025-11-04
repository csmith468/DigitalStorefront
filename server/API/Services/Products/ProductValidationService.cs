using API.Database;
using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;

namespace API.Services.Products;

public interface IProductValidationService
{
    Task<Result<bool>> ValidateProductAsync(ProductFormDto dto, Product? originalProduct = null);
    Task<Result<bool>> ValidateSubcategoryIdsAsync(List<int> subcategoryIds);
}

public class ProductValidationService : IProductValidationService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ILogger<ProductValidationService> _logger;

    public ProductValidationService(IQueryExecutor queryExecutor, ILogger<ProductValidationService> logger)
    {
        _queryExecutor = queryExecutor;
        _logger = logger;
    }
    
    public async Task<Result<bool>> ValidateProductAsync(ProductFormDto dto, Product? originalProduct = null)
    {
        if (await _queryExecutor.ExistsByFieldAsync<Product>("name", dto.Name) && (originalProduct == null || originalProduct.Name != dto.Name))
            return Result<bool>.Failure(ErrorMessages.Product.NameExists(dto.Name));
        if (await _queryExecutor.ExistsByFieldAsync<Product>("slug", dto.Slug) && (originalProduct == null || originalProduct.Slug != dto.Slug))
            return Result<bool>.Failure(ErrorMessages.Product.SlugExists(dto.Slug));
        if (dto.SubcategoryIds.Count != 0)
        {
            var subcategoryValidationResult = await ValidateSubcategoryIdsAsync(dto.SubcategoryIds);
            if (!subcategoryValidationResult.IsSuccess)
                return subcategoryValidationResult;
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ValidateSubcategoryIdsAsync(List<int> subcategoriesIds)
    {
        if (subcategoriesIds.Count == 0)
            return Result<bool>.Success(true);
        
        var distinctIds = subcategoriesIds.Distinct().ToList();
        var existingSubcategories =
            (await _queryExecutor.GetWhereInAsync<Subcategory>("subcategoryId", distinctIds)).ToList();

        if (existingSubcategories.Count == distinctIds.Count) 
            return Result<bool>.Success(true);
        
        var existingIds = existingSubcategories.Select(s => s.SubcategoryId).ToHashSet();
        var nonexistentIds = string.Join(", ", distinctIds.Where(id => !existingIds.Contains(id)).ToList());
        _logger.LogWarning("Invalid subcategoryIds attempted: {NonexistentIds}", nonexistentIds);
        return Result<bool>.Failure(ErrorMessages.Metadata.InvalidSubcategories(nonexistentIds));
    }
}