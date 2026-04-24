using FluentAssertions;
using WebApi.Application.DTOs.Orders;
using WebApi.Application.Exceptions;
using WebApi.Domain.Entities;
using WebApi.Tests.Support;

namespace WebApi.Tests;

public class OrderServiceTests : ServiceTestBase
{
    private async Task SeedBaseAsync()
    {
        Db.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
        Db.Products.Add(new Product { Id = 10, Name = "Rocket", Price = 100m, Stock = 5 });
        Db.Products.Add(new Product { Id = 11, Name = "Booster", Price = 50m, Stock = 10 });
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task PlaceAsync_HappyPath_CreatesOrderAndDecrementsStock()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var request = new CreateOrderRequest(1, new List<CreateOrderItemRequest>
        {
            new(10, 2),
            new(11, 3)
        });

        var order = await service.PlaceAsync(request, CancellationToken.None);

        order.Id.Should().BeGreaterThan(0);
        order.CustomerName.Should().Be("Alice");
        order.Items.Should().HaveCount(2);
        order.Total.Should().Be(2 * 100m + 3 * 50m);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var rocket = await Db.Products.FindAsync(10);
        var booster = await Db.Products.FindAsync(11);
        rocket!.Stock.Should().Be(3);
        booster!.Stock.Should().Be(7);
    }

    [Fact]
    public async Task PlaceAsync_SnapshotsUnitPriceEvenIfProductPriceChanges()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var order = await service.PlaceAsync(
            new CreateOrderRequest(1, new List<CreateOrderItemRequest> { new(10, 1) }),
            CancellationToken.None);

        var product = await Db.Products.FindAsync(10);
        product!.Price = 999m;
        await Db.SaveChangesAsync();

        order.Items.Single().UnitPrice.Should().Be(100m);
    }

    [Fact]
    public async Task PlaceAsync_WhenCustomerDoesNotExist_ThrowsNotFound()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var act = () => service.PlaceAsync(
            new CreateOrderRequest(999, new List<CreateOrderItemRequest> { new(10, 1) }),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(e => e.Message.Contains("Customer"));
    }

    [Fact]
    public async Task PlaceAsync_WhenProductDoesNotExist_ThrowsNotFound()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var act = () => service.PlaceAsync(
            new CreateOrderRequest(1, new List<CreateOrderItemRequest> { new(99, 1) }),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(e => e.Message.Contains("Product"));
    }

    [Fact]
    public async Task PlaceAsync_WhenStockIsInsufficient_ThrowsInsufficientStock()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var act = () => service.PlaceAsync(
            new CreateOrderRequest(1, new List<CreateOrderItemRequest> { new(10, 999) }),
            CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<InsufficientStockException>();
        thrown.Which.ProductId.Should().Be(10);
        thrown.Which.Requested.Should().Be(999);
        thrown.Which.Available.Should().Be(5);
    }

    [Fact]
    public async Task PlaceAsync_WithoutItems_FailsValidation()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var act = () => service.PlaceAsync(
            new CreateOrderRequest(1, new List<CreateOrderItemRequest>()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PlaceAsync_AggregatesSameProductQuantities_ForStockCheck()
    {
        await SeedBaseAsync();
        var service = CreateOrderService();

        var act = () => service.PlaceAsync(
            new CreateOrderRequest(1, new List<CreateOrderItemRequest>
            {
                new(10, 3),
                new(10, 3)
            }),
            CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientStockException>()
            .Where(e => e.Requested == 6 && e.Available == 5);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ThrowsNotFound()
    {
        var service = CreateOrderService();

        var act = () => service.GetByIdAsync(42, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
