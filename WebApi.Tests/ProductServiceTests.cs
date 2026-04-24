using FluentAssertions;
using WebApi.Application.Common;
using WebApi.Application.DTOs.Products;
using WebApi.Application.Exceptions;
using WebApi.Domain.Entities;
using WebApi.Tests.Support;

namespace WebApi.Tests;

public class ProductServiceTests : ServiceTestBase
{
    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsProduct()
    {
        var service = CreateProductService();

        var created = await service.CreateAsync(
            new CreateProductRequest("Rocket", "Fast ship", 1200m, 5),
            CancellationToken.None);

        created.Id.Should().BeGreaterThan(0);
        Db.Products.Should().ContainSingle(p => p.Id == created.Id && p.Name == "Rocket");
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("Valid", 0)]
    [InlineData("Valid", -1)]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException(string name, decimal price)
    {
        var service = CreateProductService();

        var act = () => service.CreateAsync(
            new CreateProductRequest(name, null, price, 5),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        var service = CreateProductService();

        var act = () => service.GetByIdAsync(42, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ListAsync_FiltersByName_CaseInsensitivePartialMatch()
    {
        Db.Products.AddRange(
            new Product { Name = "Red Rocket", Price = 10, Stock = 1 },
            new Product { Name = "Blue Booster", Price = 20, Stock = 1 },
            new Product { Name = "Rocket Fuel", Price = 30, Stock = 1 });
        await Db.SaveChangesAsync();

        var service = CreateProductService();
        var result = await service.ListAsync("rocket", new PageQuery(1, 10), CancellationToken.None);

        result.TotalItems.Should().Be(2);
        result.Items.Select(i => i.Name).Should().Contain(new[] { "Red Rocket", "Rocket Fuel" });
    }
}
