using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebApi.Application.Interfaces;
using WebApi.Application.Services;
using WebApi.Application.Validators.Customers;
using WebApi.Application.Validators.Orders;
using WebApi.Application.Validators.Products;
using WebApi.Infrastructure.Data;
using WebApi.Infrastructure.Repositories;

namespace WebApi.Tests.Support;

public class ServiceTestBase : IDisposable
{
    protected AppDbContext Db { get; }

    protected ServiceTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"RocketStore-Test-{Guid.NewGuid()}")
            .Options;
        Db = new AppDbContext(options);
    }

    protected OrderService CreateOrderService() =>
        new(
            new OrderRepository(Db),
            new ProductStockRepository(Db),
            new CustomerLookupRepository(Db),
            new CreateOrderRequestValidator(),
            NullLogger<OrderService>.Instance);

    protected ProductService CreateProductService() =>
        new(
            new ProductRepository(Db),
            new CreateProductRequestValidator(),
            new UpdateProductRequestValidator(),
            NullLogger<ProductService>.Instance);

    protected CustomerService CreateCustomerService() =>
        new(
            new CustomerRepository(Db),
            new CreateCustomerRequestValidator(),
            new UpdateCustomerRequestValidator(),
            NullLogger<CustomerService>.Instance);

    public void Dispose() => Db.Dispose();
}
