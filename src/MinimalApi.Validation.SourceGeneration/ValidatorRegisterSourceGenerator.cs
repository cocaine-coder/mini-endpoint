using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MinimalApi.Validation.SourceGeneration;

[Generator]
public class ValidatorRegisterSourceGenerator : IIncrementalGenerator
{
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();
        var compilation = context.CompilationProvider;

        context.RegisterSourceOutput(
            compilation,
            (ctx, c) =>
            {
                var registerCode = "global::System.Console.WriteLine(123);";

                foreach (var tree in c.SyntaxTrees)
                {
                    var root = tree.GetCompilationUnitRoot();

                    foreach (var member in root.Members)
                    {
                        if (member is ClassDeclarationSyntax classSyntax)
                        {
                            var baseList = classSyntax.BaseList;
                        }

                        if (
                            member
                            is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax
                        )
                        {
                            foreach (var member1 in fileScopedNamespaceDeclarationSyntax.Members)
                            {
                                if (member1 is ClassDeclarationSyntax classSyntax1)
                                {
                                    var baseList = classSyntax1.BaseList;
                                }
                            }
                        }
                    }
                }

                string source = $$"""
                using FluentValidation;
                using Microsoft.Extensions.DependencyInjection;

                namespace MinimalApi.Validation
                {
                    public static class ServiceCollectionExtensions
                    {
                        public static void RegisterAllValidators(this IServiceCollection services)
                        {
                            {{registerCode}}
                        }
                    }
                }
                """;

                ctx.AddSource("MinimalApiExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        );
    }
}
