using FluentValidation;
using Microsoft.Extensions.Logging;
using WebApi.Application.Common;
using WebApi.Application.DTOs.Products;
using WebApi.Application.Exceptions;
using WebApi.Application.Interfaces;
using WebApi.Application.Mappings;
using WebApi.Domain.Entities;

namespace WebApi.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> ListAsync(string? name, PageQuery page, CancellationToken ct)
    {
        var (items, total) = await _repository.SearchAsync(name, page.Skip, page.SafePageSize, ct);
        return new PagedResult<ProductDto>
        {
            Items = items.Select(p => p.ToDto()).ToList(),
            Page = page.SafePage,
            PageSize = page.SafePageSize,
            TotalItems = total
        };
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<Product>(id);
        return product.ToDto();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        await _createValidator.EnsureValidAsync(request, ct);

        var product = request.ToEntity();
        await _repository.AddAsync(product, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Product {ProductId} created (Name={Name}, Price={Price}).", product.Id, product.Name, product.Price);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct)
    {
        await _updateValidator.EnsureValidAsync(request, ct);

        var product = await _repository.GetByIdTrackedAsync(id, ct)
            ?? throw NotFoundException.For<Product>(id);

        request.ApplyTo(product);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Product {ProductId} updated.", product.Id);
        return product.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var product = await _repository.GetByIdTrackedAsync(id, ct)
            ?? throw NotFoundException.For<Product>(id);

        _repository.Remove(product);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Product {ProductId} deleted.", id);
    }
}
