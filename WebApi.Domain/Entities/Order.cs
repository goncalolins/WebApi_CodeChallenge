namespace WebApi.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}
