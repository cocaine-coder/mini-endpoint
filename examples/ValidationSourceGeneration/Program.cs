using System.Text.Json.Serialization;
using MinimalApi.Validation;
using ValidationSourceGeneration.ViewModels;
using ValidationSourceGeneration.ViewModels.FF;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(
                0,
                AppJsonSerializerContext.Default
            );
        });

        builder.Services.RegisterAllValidators();
        builder.Services.AddAutoValidation();

        var app = builder.Build();

        var v = app.MapGroup("/test").AddValidationFilter();
        v.MapPost(
                "/person",
                (Person p) =>
                {
                    return Results.Ok(p);
                }
            )
            .AddValidationFilter();

        v.MapPost(
                "/dog",
                (Dog p) =>
                {
                    return Results.Ok(p);
                }
            )
            .AddValidationFilter();

        //v.MapPost(
        //        "/cat",
        //        (ValidationSourceGeneration.Models.Cat p) =>
        //        {
        //            return Results.Ok(p);
        //        }
        //    )
        //    .AddValidationFilter();

        v.MapPost(
                "/cat1",
                (ValidationSourceGeneration.ViewModels.BB.Cat p) =>
                {
                    return Results.Ok(p);
                }
            )
            .AddValidationFilter();

        app.Run();
    }
}

[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(Dog))]
[JsonSerializable(typeof(ValidationSourceGeneration.ViewModels.BB.Cat))]
[JsonSerializable(typeof(ValidationSourceGeneration.Models.Cat))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
