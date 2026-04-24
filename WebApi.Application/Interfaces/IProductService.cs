using WebApi.Application.Common;
using WebApi.Application.DTOs.Products;

namespace WebApi.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> ListAsync(string? name, PageQuery page, CancellationToken ct);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
