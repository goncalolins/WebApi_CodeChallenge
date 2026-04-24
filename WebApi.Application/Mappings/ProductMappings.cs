using WebApi.Application.DTOs.Products;
using WebApi.Domain.Entities;

namespace WebApi.Application.Mappings;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Stock);

    public static Product ToEntity(this CreateProductRequest request) =>
        new()
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

    public static void ApplyTo(this UpdateProductRequest request, Product product)
    {
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
    }
}
