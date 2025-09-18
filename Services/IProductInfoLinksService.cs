using MetaSearchApp.Models;

namespace MetaSearchApp.Services;

public interface IProductInfoLinksService
{
    Task<ProductInfoLinksDto?> GetAsync(int productId);
    Task SaveAsync(ProductInfoLinksDto dto);
}
