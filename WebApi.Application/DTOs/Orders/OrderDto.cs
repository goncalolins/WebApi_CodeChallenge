namespace WebApi.Application.DTOs.Orders;

public record OrderDto(
    int Id,
    int CustomerId,
    string CustomerName,
    DateTime CreatedAt,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items);

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record CreateOrderRequest(
    int CustomerId,
    IReadOnlyList<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
    int ProductId,
    int Quantity);
