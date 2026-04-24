using FluentValidation;
using Microsoft.Extensions.Logging;
using WebApi.Application.Common;
using WebApi.Application.DTOs.Orders;
using WebApi.Application.Exceptions;
using WebApi.Application.Interfaces;
using WebApi.Application.Mappings;
using WebApi.Domain.Entities;

namespace WebApi.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductStockRepository _productRepository;
    private readonly ICustomerLookupRepository _customerRepository;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductStockRepository productRepository,
        ICustomerLookupRepository customerRepository,
        IValidator<CreateOrderRequest> validator,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PagedResult<OrderDto>> ListAsync(PageQuery page, CancellationToken ct)
    {
        var (items, total) = await _orderRepository.ListAsync(page.Skip, page.SafePageSize, ct);
        return new PagedResult<OrderDto>
        {
            Items = items.Select(o => o.ToDto()).ToList(),
            Page = page.SafePage,
            PageSize = page.SafePageSize,
            TotalItems = total
        };
    }

    public async Task<OrderDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<Order>(id);
        return order.ToDto();
    }

    public async Task<OrderDto> PlaceAsync(CreateOrderRequest request, CancellationToken ct)
    {
        await _validator.EnsureValidAsync(request, ct);

        if (!await _customerRepository.ExistsAsync(request.CustomerId, ct))
        {
            throw NotFoundException.For<Customer>(request.CustomerId);
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepository.GetTrackedByIdsAsync(productIds, ct);
        var productsById = products.ToDictionary(p => p.Id);

        var missing = productIds.Where(id => !productsById.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw NotFoundException.For<Product>(missing[0]);
        }

        var aggregatedQuantities = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        foreach (var (productId, totalQuantity) in aggregatedQuantities)
        {
            var product = productsById[productId];
            if (product.Stock < totalQuantity)
            {
                throw new InsufficientStockException(productId, totalQuantity, product.Stock);
            }
        }

        var order = new Order
        {
            CustomerId = request.CustomerId,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i =>
            {
                var product = productsById[i.ProductId];
                return new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = i.Quantity,
                    UnitPrice = product.Price
                };
            }).ToList()
        };

        foreach (var (productId, totalQuantity) in aggregatedQuantities)
        {
            productsById[productId].Stock -= totalQuantity;
        }

        await _orderRepository.AddAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        var customerName = await _customerRepository.GetNameAsync(request.CustomerId, ct) ?? string.Empty;
        foreach (var item in order.Items)
        {
            item.Product = productsById[item.ProductId];
        }

        _logger.LogInformation(
            "Order {OrderId} placed for customer {CustomerId} with {ItemCount} items (total {Total}).",
            order.Id, order.CustomerId, order.Items.Count, order.Items.Sum(i => i.UnitPrice * i.Quantity));

        return new OrderDto(
            order.Id,
            order.CustomerId,
            customerName,
            order.CreatedAt,
            order.Items.Sum(i => i.UnitPrice * i.Quantity),
            order.Items.Select(i => i.ToDto()).ToList());
    }
}
