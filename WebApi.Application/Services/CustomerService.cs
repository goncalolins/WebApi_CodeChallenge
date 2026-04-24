using FluentValidation;
using Microsoft.Extensions.Logging;
using WebApi.Application.Common;
using WebApi.Application.DTOs.Customers;
using WebApi.Application.Exceptions;
using WebApi.Application.Interfaces;
using WebApi.Application.Mappings;
using WebApi.Domain.Entities;

namespace WebApi.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository repository,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator,
        ILogger<CustomerService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<CustomerDto>> ListAsync(string? email, PageQuery page, CancellationToken ct)
    {
        var (items, total) = await _repository.SearchAsync(email, page.Skip, page.SafePageSize, ct);
        return new PagedResult<CustomerDto>
        {
            Items = items.Select(c => c.ToDto()).ToList(),
            Page = page.SafePage,
            PageSize = page.SafePageSize,
            TotalItems = total
        };
    }

    public async Task<CustomerDetailsDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var customer = await _repository.GetByIdWithOrdersAsync(id, ct)
            ?? throw NotFoundException.For<Customer>(id);
        return customer.ToDetailsDto();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        await _createValidator.EnsureValidAsync(request, ct);

        if (await _repository.ExistsByEmailAsync(request.Email, excludeId: null, ct))
        {
            throw new ConflictException($"A customer with email '{request.Email}' already exists.");
        }

        var customer = request.ToEntity();
        await _repository.AddAsync(customer, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Customer {CustomerId} created (Email={Email}).", customer.Id, customer.Email);
        return customer.ToDto();
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct)
    {
        await _updateValidator.EnsureValidAsync(request, ct);

        var customer = await _repository.GetByIdTrackedAsync(id, ct)
            ?? throw NotFoundException.For<Customer>(id);

        var emailChanged = !string.Equals(customer.Email, request.Email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged && await _repository.ExistsByEmailAsync(request.Email, excludeId: id, ct))
        {
            throw new ConflictException($"A customer with email '{request.Email}' already exists.");
        }

        request.ApplyTo(customer);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Customer {CustomerId} updated.", customer.Id);
        return customer.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var customer = await _repository.GetByIdTrackedAsync(id, ct)
            ?? throw NotFoundException.For<Customer>(id);

        if (await _repository.HasOrdersAsync(id, ct))
        {
            throw new ConflictException($"Customer {id} cannot be deleted because they have existing orders.");
        }

        _repository.Remove(customer);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Customer {CustomerId} deleted.", id);
    }
}
