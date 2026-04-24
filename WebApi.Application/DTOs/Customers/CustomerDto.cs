namespace WebApi.Application.DTOs.Customers;

public record CustomerDto(
    int Id,
    string Name,
    string Email);

public record CustomerDetailsDto(
    int Id,
    string Name,
    string Email,
    IReadOnlyList<CustomerOrderDto> Orders);

public record CustomerOrderDto(
    int Id,
    DateTime CreatedAt,
    decimal Total,
    int ItemCount);

public record CreateCustomerRequest(
    string Name,
    string Email);

public record UpdateCustomerRequest(
    string Name,
    string Email);
