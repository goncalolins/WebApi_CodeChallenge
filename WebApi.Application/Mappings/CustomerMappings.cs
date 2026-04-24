using WebApi.Application.DTOs.Customers;
using WebApi.Domain.Entities;

namespace WebApi.Application.Mappings;

public static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.Name, customer.Email);

    public static CustomerDetailsDto ToDetailsDto(this Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Orders.Select(o => new CustomerOrderDto(
                o.Id,
                o.CreatedAt,
                o.Items.Sum(i => i.UnitPrice * i.Quantity),
                o.Items.Count)).ToList());

    public static Customer ToEntity(this CreateCustomerRequest request) =>
        new()
        {
            Name = request.Name,
            Email = request.Email
        };

    public static void ApplyTo(this UpdateCustomerRequest request, Customer customer)
    {
        customer.Name = request.Name;
        customer.Email = request.Email;
    }
}
