using API.Models;
using API.Models.DboTables;
using API.Setup;

namespace API.Services;

public interface IProductService
{
    Task<Result<List<Product>>> GetAllProductsAsync();
    Task<Result<Product>> GetProductByIdAsync(int productId);
    Task<Result<List<Product>>> GetProductsBySubcategoryAsync(string subcategorySlug);
}

public class ProductService(ISharedContainer container) : BaseService(container)
{
    public async Task<Result<List<Product>>> GetAllProductsAsync()
    {
        var products = (await Dapper.GetAllAsync<Product>()).ToList();
        return Result<List<Product>>.Success(products);
    }

    public async Task<Result<Product>> GetProductByIdAsync(int productId)
    {
        var product = await Dapper.GetByIdAsync<Product>(productId);
        return product == null 
            ? Result<Product>.Failure($"Product ID {productId} not found.", statusCode: 404) 
            : Result<Product>.Success(product);
    }

    public async Task<Result<List<Product>>> GetProductsBySubcategoryAsync(string subcategorySlug)
    {
        var categoryExists = await Dapper.ExistsByFieldAsync<Subcategory>("slug", subcategorySlug);
        if (!categoryExists)
            return Result<List<Product>>.Failure($"Subcategory slug {subcategorySlug} not found.", statusCode: 404);

        var products = (await Dapper.QueryAsync<Product>(
            """
                SELECT p.*
                FROM dbo.product p
                JOIN dbo.productSubcategory ps ON p.productId = ps.productId
                join dbo.subcategory s ON s.subcategoryId = ps.subcategoryId
                WHERE s.slug = @subcategorySlug
            """,
            new { subcategorySlug })).ToList();


        return Result<List<Product>>.Success(products);
    }
}