using API.Database;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;

namespace API.Services.Products;

public interface IProductMappingService
{
    Task<ProductDetailDto> ToProductDetailDtoAsync(Product product, CancellationToken ct = default);
    Task<Result<List<ProductDto>>> ToProductDtosAsync(List<Product> products, CancellationToken ct = default);
}

public class ProductMappingService : IProductMappingService
{
    private readonly IMapper _mapper;
    private readonly IProductImageService _productImageService;
    private readonly IQueryExecutor _queryExecutor;

    public ProductMappingService(IMapper mapper, IProductImageService productImageService, IQueryExecutor queryExecutor)
    {
        _mapper = mapper;
        _productImageService = productImageService;
        _queryExecutor = queryExecutor;
    }
    
    public async Task<ProductDetailDto> ToProductDetailDtoAsync(Product product, CancellationToken ct = default)
    {
        var detailDto = _mapper.Map<Product, ProductDetailDto>(product);

        var imagesResult = await _productImageService.GetAllProductImagesAsync(product.ProductId, ct);
        if (imagesResult.IsSuccess)
            detailDto.Images = imagesResult.Data;

        var productSubcategories = await _queryExecutor.GetByFieldAsync<ProductSubcategory>("productId", product.ProductId, ct);
        var subcategories = await _queryExecutor.GetWhereInAsync<Subcategory>("subcategoryId",
            productSubcategories.Select(s => s.SubcategoryId).ToList(), ct);
        detailDto.Subcategories = subcategories.Select(s => _mapper.Map<Subcategory, SubcategoryDto>(s)).ToList();
        
        var productTags = (await _queryExecutor.GetByFieldAsync<ProductTag>("productId", product.ProductId, ct)).ToList();
        if (productTags.Count != 0)
        {
            var tags = await _queryExecutor.GetWhereInAsync<Tag>("tagId", productTags.Select(pt => pt.TagId).ToList(), ct);
            detailDto.Tags = tags.Select(t => _mapper.Map<Tag, TagDto>(t)).ToList();
        }
        
        var priceType = PriceTypes.All.FirstOrDefault(pt => pt.PriceTypeId == product.PriceTypeId);
        detailDto.PriceIcon = priceType != null ? priceType.Icon : "";
        
        return detailDto;
    }

    public async Task<Result<List<ProductDto>>> ToProductDtosAsync(List<Product> products, CancellationToken ct = default)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        var primaryImages = await _productImageService.GetPrimaryImagesForProductIdsAsync(productIds, ct);

        var productDtos = products.Select(p =>
        {
            var productDto = _mapper.Map<Product, ProductDto>(p);
            productDto.PrimaryImage = primaryImages.Data.FirstOrDefault(pi => pi.ProductId == p.ProductId);
            var priceType = PriceTypes.All.FirstOrDefault(pt => pt.PriceTypeId == p.PriceTypeId);
            productDto.PriceIcon = priceType != null ? priceType.Icon : "";
            return productDto;
        }).ToList();
        return Result<List<ProductDto>>.Success(productDtos);
    }
}