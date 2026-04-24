using WebApi.Domain.Entities;

namespace WebApi.Application.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(string? nameFilter, int skip, int take, CancellationToken ct);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<Product?> GetByIdTrackedAsync(int id, CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
    void Remove(Product product);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
