using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MinimalApi.Validation.SourceGeneration;

[Generator]
public class ValidatorRegisterSourceGenerator : IIncrementalGenerator
{
    string RegisterCode = "";

    void SearchInMember(MemberDeclarationSyntax member, string currentPath)
    {
        if (member is ClassDeclarationSyntax classSyntax)
        {
            var baseList = classSyntax.BaseList;
            if (baseList != null)
            {
                foreach (var baseType in baseList.Types)
                {
                    if (
                        baseType.Type is GenericNameSyntax genericNameSyntax
                        && genericNameSyntax.Identifier.Text == "AbstractValidator"
                    )
                    {
                        var argument = genericNameSyntax.TypeArgumentList.Arguments[0];

                        var validateTarget = argument.ToString();
                        var validator = classSyntax.Identifier.Text;

                        RegisterCode +=
                            $"services.AddScoped(IValidator<{validateTarget}, {currentPath + "." + validator}>);\n";
                    }
                }
            }
            else
            {
                // class嵌套
                foreach (var item in classSyntax.Members)
                {
                    SearchInMember(item, $"${currentPath}.{classSyntax.Identifier.Text}");
                }
            }
        }
    }

    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Launch();
        var compilation = context.CompilationProvider;

        context.RegisterSourceOutput(
            compilation,
            (ctx, c) =>
            {
                // 寻找继承AbstractValidator<T>的类
                foreach (var tree in c.SyntaxTrees)
                {
                    var root = tree.GetCompilationUnitRoot();

                    foreach (var member in root.Members)
                    {
                        if (member is BaseNamespaceDeclarationSyntax namespaceDeclaration)
                        {
                            foreach (var item in namespaceDeclaration.Members)
                            {
                                SearchInMember(item, namespaceDeclaration.Name.ToString());
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
                            {{RegisterCode}}
                        }
                    }
                }
                """;

                ctx.AddSource("MinimalApiExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        );
    }
}
