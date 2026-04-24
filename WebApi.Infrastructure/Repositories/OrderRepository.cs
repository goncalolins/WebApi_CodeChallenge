using Microsoft.EntityFrameworkCore;
using WebApi.Application.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Infrastructure.Data;

namespace WebApi.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Order> Items, int Total)> ListAsync(int skip, int take, CancellationToken ct)
    {
        var baseQuery = _db.Orders.AsNoTracking();
        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct) =>
        await _db.Orders.AddAsync(order, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}

public class ProductStockRepository : IProductStockRepository
{
    private readonly AppDbContext _db;

    public ProductStockRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetTrackedByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.ToList();
        return await _db.Products
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(ct);
    }
}

public class CustomerLookupRepository : ICustomerLookupRepository
{
    private readonly AppDbContext _db;

    public CustomerLookupRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        _db.Customers.AnyAsync(c => c.Id == id, ct);

    public Task<string?> GetNameAsync(int id, CancellationToken ct) =>
        _db.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);
}
