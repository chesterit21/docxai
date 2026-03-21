namespace Api.Services.Core.Models.Errors;

// Api.Services.Core/Models/Errors/ErrorParser.cs
public static class ErrorParser
{
    public static ProviderErrorType ParseHttpStatusCode(int statusCode)
    {
        return statusCode switch
        {
            401 or 403 => ProviderErrorType.Authentication,
            429 => ProviderErrorType.RateLimited,
            400 or 422 => ProviderErrorType.InvalidRequest,
            >= 500 => ProviderErrorType.ServerError,
            _ => ProviderErrorType.Unknown
        };
    }
}