using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalApi.Validation;

[RequiresDynamicCode("don't worry, aot fully supported.")]
internal class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        foreach (var arg in context.Arguments)
        {
            if (arg != null)
            {
                var services = context.HttpContext.RequestServices;
                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());

                if (services.GetService(validatorType) is not IValidator validator)
                {
                    continue;
                }

                var validationContext = new ValidationContext<object>(arg);
                var validationResult = await validator.ValidateAsync(
                    validationContext,
                    context.HttpContext.RequestAborted
                );

                if (!validationResult.IsValid)
                {
                    var validationFilterConfiguration = services.GetRequiredService<
                        IOptions<ValidationFilterConfiguration>
                    >();
                    return validationFilterConfiguration.Value.ValidationResultCreator(
                        validationResult,
                        context.HttpContext
                    );
                }
            }
        }

        return await next(context);
    }
}

internal class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        foreach (var arg in context.Arguments)
        {
            if (arg != null && arg is T a)
            {
                var services = context.HttpContext.RequestServices;
                var validator = services.GetRequiredService<IValidator<T>>();
                var validationResult = await validator.ValidateAsync(
                    a,
                    context.HttpContext.RequestAborted
                );

                if (!validationResult.IsValid)
                {
                    var validationFilterConfiguration = services.GetRequiredService<
                        IOptions<ValidationFilterConfiguration>
                    >();
                    return validationFilterConfiguration.Value.ValidationResultCreator(
                        validationResult,
                        context.HttpContext
                    );
                }
            }
        }

        return await next(context);
    }
}
