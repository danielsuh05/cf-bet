using System.Net;

namespace backend.utils;

public class RestException(HttpStatusCode? code, string? errorMessage = "") : Exception
{
    public string? ErrorMessage { get; } = errorMessage;
    public HttpStatusCode? Code { get; } = code;
}