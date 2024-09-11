using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalApi.Validation;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutoValidation(
        this IServiceCollection services,
        Action<ValidationFilterConfiguration>? configurationBuilder
    )
    {
        var config = new ValidationFilterConfiguration();
        configurationBuilder?.Invoke(config);

        services.ConfigureOptions(config);
        return services;
    }
}

public static class RouterBuilderExtensions
{
    public static RouteGroupBuilder AddValidationFilter(this RouteGroupBuilder builder) =>
        builder.AddEndpointFilter<ValidationFilter>();

    public static RouteHandlerBuilder AddValidationFilter(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValidationFilter>();
}
