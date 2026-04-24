using FluentValidation;
using FluentValidation.Results;
using WebApi.Application.Exceptions;
using ValidationException = WebApi.Application.Exceptions.ValidationException;

namespace WebApi.Application.Common;

public static class ValidationExtensions
{
    public static async Task EnsureValidAsync<T>(this IValidator<T> validator, T instance, CancellationToken ct = default)
    {
        ValidationResult result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid) return;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        throw new ValidationException(errors);
    }
}
