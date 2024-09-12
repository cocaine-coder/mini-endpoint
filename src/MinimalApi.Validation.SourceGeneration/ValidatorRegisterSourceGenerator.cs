using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MinimalApi.Validation.SourceGeneration;

[Generator]
public class ValidatorRegisterSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 从 <see cref="MemberDeclarationSyntax"/> member 中提取继承 AbstractValidator 类的子类，生成注册代码
    /// </summary>
    /// <param name="member"></param>
    /// <param name="currentPath">记录子类完整路径, 防止子类为嵌套</param>
    /// <param name="code">生成的代码</param>
    void SearchInMember(MemberDeclarationSyntax member, string currentPath, ref string code)
    {
        if (member is ClassDeclarationSyntax classSyntax)
        {
            var baseList = classSyntax.BaseList;
            if (baseList != null)
            {
                foreach (var baseType in baseList.Types)
                {
                    // 判断继承了AbstractValidator
                    if (
                        baseType.Type is GenericNameSyntax genericNameSyntax
                        && genericNameSyntax.Identifier.Text == "AbstractValidator"
                    )
                    {
                        var validateTarget = genericNameSyntax
                            .TypeArgumentList.Arguments[0]
                            .ToString();
                        var validator = classSyntax.Identifier.Text;

                        code +=
                            $"services.AddScoped<IValidator<{validateTarget}>, {currentPath + "." + validator}>();\n";
                    }
                }
            }
            else
            {
                // 递归嵌套的子类
                foreach (var item in classSyntax.Members)
                {
                    SearchInMember(item, $"${currentPath}.{classSyntax.Identifier.Text}", ref code);
                }
            }
        }
    }

    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilation = context.CompilationProvider;

        context.RegisterSourceOutput(
            compilation,
            (ctx, c) =>
            {
                // 收集所有的注册方法
                var functions = new List<string>();

                // 遍历代码树
                for (var i = 0; i < c.SyntaxTrees.Count(); i++)
                {
                    var tree = c.SyntaxTrees.ElementAt(i);
                    var root = tree.GetCompilationUnitRoot();

                    // 记录但文件多个namespace代码
                    var namespaceCodes = string.Empty;

                    foreach (var member in root.Members)
                    {
                        //TODO : 需要解析顶级语句中的代码

                        var code = string.Empty;
                        if (member is BaseNamespaceDeclarationSyntax namespaceDeclaration)
                        {
                            foreach (var item in namespaceDeclaration.Members)
                            {
                                SearchInMember(
                                    item,
                                    namespaceDeclaration.Name.ToString(),
                                    ref code
                                );
                            }

                            if (!string.IsNullOrEmpty(code))
                            {
                                var countOfFunctions = functions.Count;
                                var namespaceName = namespaceDeclaration.Name;

                                // 定义类名，防止类名重复
                                var className =
                                    $"ServiceCollectionValidationExtensions{countOfFunctions}";

                                var functionName = "RegisterAllValidators";
                                functions.Add(
                                    $"{namespaceName}.{className}.{functionName}(services);"
                                );

                                namespaceCodes += $$"""
                                    namespace {{namespaceName}}
                                    {
                                        public static class {{className}}
                                        {
                                            public static void {{functionName}}(IServiceCollection services)
                                            {
                                                {{code}}
                                            }
                                        }
                                    }
                                    
                                    """;

                                code = string.Empty;
                            }
                        }
                    }

                    // 收集using，防止子类，与验证类无法引用
                    var usings = new HashSet<string>
                    {
                        "using FluentValidation;",
                        "using Microsoft.Extensions.DependencyInjection;"
                    };
                    foreach (var item in root.Usings)
                    {
                        usings.Add($"using {item.Name};");
                    }

                    // 组装单文件注册代码
                    if (!string.IsNullOrEmpty(namespaceCodes))
                    {
                        string source = $$"""
                            // <auto-generated/>

                            {{string.Join("\r\n", usings)}}

                            {{string.Join("\r\n", namespaceCodes)}}
                            """;

                        ctx.AddSource(
                            $"MinimalApiExtensions.Validation.{i}.g.cs",
                            SourceText.From(source, Encoding.UTF8)
                        );

                        namespaceCodes = string.Empty;
                    }
                }

                var extensionCode = $$"""
                    // <auto-generated/>

                    using Microsoft.Extensions.DependencyInjection;

                    namespace MinimalApi.Validation
                    {
                        public static class ServiceCollectionValidationExtensions
                        {
                            public static void RegisterAllValidators(this IServiceCollection services)
                            {
                                {{string.Join("\r\n", functions)}}
                            }
                        }
                    }
                    """;
                ctx.AddSource(
                    "MinimalApiExtensions.Validation.g.cs",
                    SourceText.From(extensionCode, Encoding.UTF8)
                );
            }
        );
    }
}
