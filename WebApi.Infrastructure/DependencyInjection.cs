using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.Interfaces;
using WebApi.Infrastructure.Data;
using WebApi.Infrastructure.Repositories;

namespace WebApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string inMemoryDatabaseName = "RocketStore")
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(inMemoryDatabaseName));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductStockRepository, ProductStockRepository>();
        services.AddScoped<ICustomerLookupRepository, CustomerLookupRepository>();

        return services;
    }
}
