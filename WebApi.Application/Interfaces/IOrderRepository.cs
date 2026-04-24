using WebApi.Domain.Entities;

namespace WebApi.Application.Interfaces;

public interface IOrderRepository
{
    Task<(IReadOnlyList<Order> Items, int Total)> ListAsync(int skip, int take, CancellationToken ct);
    Task<Order?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public interface IProductStockRepository
{
    Task<IReadOnlyList<Product>> GetTrackedByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
}

public interface ICustomerLookupRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<string?> GetNameAsync(int id, CancellationToken ct);
}
