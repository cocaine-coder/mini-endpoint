using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace MinimalApi.Validation;

public class ValidationFilterConfiguration
{
    public Func<ValidationResult, HttpContext, object> ValidationResultCreator { get; set; } =
        (validationResult, httpContext) =>
        {
            httpContext.Response.StatusCode = 400;
            return validationResult.ToDictionary();
        };
}
