using WebApi.Application.Common;
using WebApi.Application.DTOs.Customers;

namespace WebApi.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> ListAsync(string? email, PageQuery page, CancellationToken ct);
    Task<CustomerDetailsDto> GetByIdAsync(int id, CancellationToken ct);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
