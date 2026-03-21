namespace Api.Services.Core.Models.Errors;

/// <summary>
/// Exception thrown when LLM provider encounters an error.
/// </summary>
public class ProviderException : Exception
{
    public string ProviderName { get; }
    public int? StatusCode { get; }
    public string? RawResponse { get; }
    public ProviderErrorType ErrorType { get; }

    public ProviderException(
        string providerName,
        string message,
        Exception? innerException = null,
        int? statusCode = null,
        string? rawResponse = null,
        ProviderErrorType errorType = ProviderErrorType.Unknown)
        : base($"[{providerName}] {message}", innerException)
    {
        ProviderName = providerName;
        StatusCode = statusCode;
        RawResponse = rawResponse;
        ErrorType = errorType;
    }
}

public enum ProviderErrorType
{
    Unknown,
    Authentication,
    RateLimited,
    InvalidRequest,
    ServerError,
    Timeout,
    NetworkError,
    ParseError
}
