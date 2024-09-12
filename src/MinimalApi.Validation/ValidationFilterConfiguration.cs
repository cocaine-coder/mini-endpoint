using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace MinimalApi.Validation;

/// <summary>
/// config http response when validate error
/// </summary>
public class ValidationFilterConfiguration
{
    public Func<ValidationResult, HttpContext, object> ValidationResultCreator { get; set; } =
        CreateDefaultUnInvalideHttpResult;

    public static IDictionary<string, string[]> CreateDefaultUnInvalideHttpResult(
        ValidationResult validationResult,
        HttpContext httpContext
    )
    {
        httpContext.Response.StatusCode = 400;
        return validationResult.ToDictionary();
    }
}
