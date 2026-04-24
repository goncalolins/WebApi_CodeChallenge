using FluentAssertions;
using WebApi.Application.DTOs.Customers;
using WebApi.Application.Exceptions;
using WebApi.Domain.Entities;
using WebApi.Tests.Support;

namespace WebApi.Tests;

public class CustomerServiceTests : ServiceTestBase
{
    [Fact]
    public async Task CreateAsync_InvalidEmail_Throws()
    {
        var service = CreateCustomerService();

        var act = () => service.CreateAsync(
            new CreateCustomerRequest("Alice", "not-an-email"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingOrders_ThrowsConflict()
    {
        var customer = new Customer { Name = "Alice", Email = "alice@example.com" };
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();

        Db.Orders.Add(new Order
        {
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = 10 }
            }
        });
        await Db.SaveChangesAsync();

        var service = CreateCustomerService();

        var act = () => service.DeleteAsync(customer.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsConflict()
    {
        Db.Customers.Add(new Customer { Name = "Alice", Email = "alice@example.com" });
        await Db.SaveChangesAsync();

        var service = CreateCustomerService();

        var act = () => service.CreateAsync(
            new CreateCustomerRequest("Another Alice", "alice@example.com"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_IsCaseInsensitive()
    {
        Db.Customers.Add(new Customer { Name = "Alice", Email = "alice@example.com" });
        await Db.SaveChangesAsync();

        var service = CreateCustomerService();

        var act = () => service.CreateAsync(
            new CreateCustomerRequest("Another", "ALICE@Example.COM"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateAsync_EmailCollidesWithAnotherCustomer_ThrowsConflict()
    {
        var alice = new Customer { Name = "Alice", Email = "alice@example.com" };
        var bob = new Customer { Name = "Bob", Email = "bob@example.com" };
        Db.Customers.AddRange(alice, bob);
        await Db.SaveChangesAsync();

        var service = CreateCustomerService();

        var act = () => service.UpdateAsync(
            bob.Id,
            new UpdateCustomerRequest("Bob", "alice@example.com"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateAsync_SameCustomerSameEmail_IsAllowed()
    {
        var alice = new Customer { Name = "Alice", Email = "alice@example.com" };
        Db.Customers.Add(alice);
        await Db.SaveChangesAsync();

        var service = CreateCustomerService();

        var updated = await service.UpdateAsync(
            alice.Id,
            new UpdateCustomerRequest("Alice Renamed", "alice@example.com"),
            CancellationToken.None);

        updated.Name.Should().Be("Alice Renamed");
        updated.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task DeleteAsync_WithoutOrders_RemovesCustomer()
    {
        var customer = new Customer { Name = "Bob", Email = "bob@example.com" };
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();

        var service = CreateCustomerService();
        await service.DeleteAsync(customer.Id, CancellationToken.None);

        Db.Customers.Should().NotContain(c => c.Id == customer.Id);
    }
}
