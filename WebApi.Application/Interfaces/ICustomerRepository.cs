using WebApi.Domain.Entities;

namespace WebApi.Application.Interfaces;

public interface ICustomerRepository
{
    Task<(IReadOnlyList<Customer> Items, int Total)> SearchAsync(string? emailFilter, int skip, int take, CancellationToken ct);
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct);
    Task<Customer?> GetByIdWithOrdersAsync(int id, CancellationToken ct);
    Task<Customer?> GetByIdTrackedAsync(int id, CancellationToken ct);
    Task<bool> HasOrdersAsync(int customerId, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId, CancellationToken ct);
    Task AddAsync(Customer customer, CancellationToken ct);
    void Remove(Customer customer);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
