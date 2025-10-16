using API.Extensions;
using API.Models.Dtos;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("image")]
public class ImageController(ISharedContainer container) : BaseController(container)
{
    private IProductImageService productImageService => DepInj<IProductImageService>();
    
    [HttpPost("product/{productId}")]
    public async Task<ActionResult<ProductImageDto>> UploadProductImage(int productId, [FromForm] AddProductImageDto dto)
    {
        return (await productImageService.AddProductImageAsync(productId, dto)).ToActionResult();
    }

    [HttpDelete("product/{productId}")]
    public async Task<ActionResult<bool>> DeleteProductImage(int productId)
    {
        return (await productImageService.DeleteProductImageAsync(productId)).ToActionResult();
    }

    [HttpPut("product/{productId}/image/{imageId}/set-primary")]
    public async Task<ActionResult<bool>> SetProductImagePrimary(int productId, int imageId)
    {
        return (await productImageService.SetPrimaryImageAsync(productId, imageId)).ToActionResult();
    }
}