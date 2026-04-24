using WebApi.Application.Common;
using WebApi.Application.DTOs.Orders;

namespace WebApi.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> ListAsync(PageQuery page, CancellationToken ct);
    Task<OrderDto> GetByIdAsync(int id, CancellationToken ct);
    Task<OrderDto> PlaceAsync(CreateOrderRequest request, CancellationToken ct);
}
