namespace ECommerce.Api.Services;

/// <summary>
/// Raised by services when a request fails a business rule that maps to specific fields,
/// so controllers can return a standard validation ProblemDetails response.
/// </summary>
public sealed class RequestValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public RequestValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }
}
