using WebApi.Application.DTOs.Orders;
using WebApi.Domain.Entities;

namespace WebApi.Application.Mappings;

public static class OrderMappings
{
    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.Customer?.Name ?? string.Empty,
            order.CreatedAt,
            order.Items.Sum(i => i.UnitPrice * i.Quantity),
            order.Items.Select(i => i.ToDto()).ToList());

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(
            item.Id,
            item.ProductId,
            item.Product?.Name ?? string.Empty,
            item.Quantity,
            item.UnitPrice,
            item.UnitPrice * item.Quantity);
}
