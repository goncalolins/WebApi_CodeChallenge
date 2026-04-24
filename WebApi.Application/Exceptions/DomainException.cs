namespace WebApi.Application.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For<T>(object id) =>
        new($"{typeof(T).Name} with id '{id}' was not found.");
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class InsufficientStockException : DomainException
{
    public int ProductId { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException(int productId, int requested, int available)
        : base($"Insufficient stock for product {productId}. Requested: {requested}, available: {available}.")
    {
        ProductId = productId;
        Requested = requested;
        Available = available;
    }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
