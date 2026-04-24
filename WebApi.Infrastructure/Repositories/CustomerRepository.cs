using Microsoft.EntityFrameworkCore;
using WebApi.Application.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Infrastructure.Data;

namespace WebApi.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Customer> Items, int Total)> SearchAsync(
        string? emailFilter, int skip, int take, CancellationToken ct)
    {
        var query = _db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(emailFilter))
        {
            var term = emailFilter.Trim().ToLower();
            query = query.Where(c => c.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Customer?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Customer?> GetByIdWithOrdersAsync(int id, CancellationToken ct) =>
        _db.Customers
            .AsNoTracking()
            .Include(c => c.Orders)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Customer?> GetByIdTrackedAsync(int id, CancellationToken ct) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> HasOrdersAsync(int customerId, CancellationToken ct) =>
        _db.Orders.AnyAsync(o => o.CustomerId == customerId, ct);

    public Task<bool> ExistsByEmailAsync(string email, int? excludeId, CancellationToken ct)
    {
        var normalized = email.Trim().ToLower();
        var query = _db.Customers.Where(c => c.Email.ToLower() == normalized);
        if (excludeId is int id)
        {
            query = query.Where(c => c.Id != id);
        }
        return query.AnyAsync(ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct) =>
        await _db.Customers.AddAsync(customer, ct);

    public void Remove(Customer customer) => _db.Customers.Remove(customer);

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
