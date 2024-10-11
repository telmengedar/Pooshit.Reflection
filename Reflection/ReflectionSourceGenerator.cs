using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pooshit.Reflection;

[Generator]
public class ReflectionSourceGenerator : IIncrementalGenerator {
    bool FindClasses(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration) {
            return classDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToFullString() == "ReflectType" || a.Name.ToFullString()=="ReflectTypes"));
        }

        return false;
    }

    IEnumerable<IPropertySymbol> ExtractProperties(ITypeSymbol symbol) {
        if (symbol.BaseType != null)
            foreach (IPropertySymbol member in ExtractProperties(symbol.BaseType))
                yield return member;

        foreach (IPropertySymbol member in symbol.GetMembers()
                                                 .OfType<IPropertySymbol>()
                                                 .Where(s => s.DeclaredAccessibility.HasFlag(Accessibility.Public))) {
            if (member.IsIndexer)
                continue;
            yield return member;
        }
    }
    
    static string Convert(KeyValuePair<string, TypedConstant> pair)
    {
        if (pair.Value is { Kind: TypedConstantKind.Array, IsNull: false })
        {
            return $"new[] {pair.Value.ToCSharpString()}";
        }

        return $"{pair.Value.ToCSharpString()}";
    }

    string ToIdentifierString(string typeString) {
        return new string(typeString.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
    }
    
    void Generate(SourceProductionContext context, ImmutableArray<ITypeSymbol> types) {
        foreach (ITypeSymbol type in new HashSet<ITypeSymbol>(types, SymbolEqualityComparer.Default)) {
            string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string identifierName = ToIdentifierString(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            IPropertySymbol[] properties = ExtractProperties(type)
                                           .GroupBy(t => t.Name)
                                           .Select(t => t.Last())
                                           .ToArray();

            string version = typeof(Reflection).Assembly.GetName().Version.ToString();
            string source = $@"
using System;
using System.Linq;

namespace Pooshit.Reflection;

[System.CodeDom.Compiler.GeneratedCode(""Pooshit.Reflection"", ""{version}"")]
public static class {identifierName}Extensions
{{
    [global::System.Runtime.CompilerServices.ModuleInitializer]
    public static void Bootstrap()
    {{
        Reflection.AddModel(new Model{{
            Type = typeof({typeName}),
            Properties = new IPropertyInfo[]{{{string.Join(",", properties.Select(p=>$@"
                new PropertyInfo<{typeName},{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(
                    ""{p.Name}"",
                    new Attribute[]{{{GenerateAttributes(p.GetAttributes())}}},
                    {GenerateGetterAndSetter(typeName, p)})
            "))}}}
        }});
    }}
}}
";
            context.AddSource($"Reflection.{identifierName}.Extensions.g", source);
        }
    }
    
    static string GenerateAttributes(ImmutableArray<AttributeData> attributes) {
        return string.Join(", ", attributes
                                 .Select(o => {
                                             if (o.AttributeConstructor == null || o.AttributeClass == null)
                                                 return null;
                                             IEnumerable<string> parameters = o.AttributeConstructor.Parameters
                                                                               .Select((parameter, i) =>
                                                                                           new KeyValuePair<string, TypedConstant>(parameter.Name, o.ConstructorArguments[i]))
                                                                               .Select(Convert);

                                             string namedArguments = "";
                                             if (o.NamedArguments.Any())
                                                 namedArguments = $"{{{string.Join(", ", o.NamedArguments.Select(a => $"{a.Key} = {a.Value.ToCSharpString()}"))}}}";

                                             return
                                                 $"new {o.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}({string.Join(", ", parameters)}){namedArguments}";
                                         }).Where(d => d != null));
    }
    
    string GenerateGetterAndSetter(string typeName, IPropertySymbol propertySymbol)
    {
        StringBuilder sb = new();
        if (propertySymbol.GetMethod != null &&
            propertySymbol.GetMethod.DeclaredAccessibility.HasFlag(Accessibility.Public)) {
            if (propertySymbol.IsStatic)
                sb.Append($"instance => {typeName}.{propertySymbol.Name}");
            else sb.Append($"instance => instance.{propertySymbol.Name}");
        }
        else
        {
            sb.Append("null");
        }

        sb.Append(", ");

        if (propertySymbol.SetMethod != null &&
            propertySymbol.SetMethod.DeclaredAccessibility.HasFlag(Accessibility.Public)) {
            if (propertySymbol.IsStatic)
                sb.Append($"(instance, value) => {typeName}.{propertySymbol.Name} = value");
            else sb.Append($"(instance, value) => instance.{propertySymbol.Name} = value");
        }
        else
        {
            sb.Append("null");
        }

        return sb.ToString();
    }
    IEnumerable<ITypeSymbol> GetReflectionInfo(GeneratorSyntaxContext context, CancellationToken token) {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
            yield break;

        if (classDeclaration.AttributeLists.Any(a => a.Attributes.Any(at => at.Name.ToFullString() == "ReflectTypes"))) {
            foreach (PropertyDeclarationSyntax member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>()) {
                SymbolInfo symbol = context.SemanticModel.GetSymbolInfo(member.Type);
                if (symbol.Symbol is ITypeSymbol typeSymbol)
                    yield return typeSymbol;
            }
        }
        else {
            INamedTypeSymbol namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (namedTypeSymbol is ITypeSymbol typeSymbol)
                yield return typeSymbol;
        }
        /*SymbolInfo symbol;ExpressionSyntax symbolExpression = argument.Expression;
            if (symbolExpression is TypeOfExpressionSyntax typeOf)
                yield return context.SemanticModel.GetSymbolInfo(typeOf.Type, token).Symbol as ITypeSymbol;
            else if (symbolExpression is InvocationExpressionSyntax argumentInvocation) {
                if (argumentInvocation.Expression is MemberAccessExpressionSyntax memberAccess) {
                    symbol = context.SemanticModel.GetSymbolInfo(memberAccess.Expression, token);
                    if (symbol.Symbol is ILocalSymbol localSymbol)
                        yield return localSymbol.Type;
                }
            }
            else if (symbolExpression is IdentifierNameSyntax identifier) {
                symbol = context.SemanticModel.GetSymbolInfo(identifier);
                SyntaxNode declaringNode = symbol.Symbol?.DeclaringSyntaxReferences[0].GetSyntax(token);
                if (declaringNode != null) {
                    ExpressionSyntax value = (declaringNode as VariableDeclaratorSyntax)?.Initializer?.Value;
                    if (value is InvocationExpressionSyntax declarationInvocation) {
                        if (declarationInvocation.Expression is MemberAccessExpressionSyntax memberAccess) {
                            symbol = context.SemanticModel.GetSymbolInfo(memberAccess.Expression, token);
                            if (symbol.Symbol is ILocalSymbol localSymbol) {
                                yield return localSymbol.Type;
                            }
                        }
                    }
                }
            }*/
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValuesProvider<ITypeSymbol> types = context.SyntaxProvider.CreateSyntaxProvider(
                                                                                                   FindClasses,
                                                                                                   GetReflectionInfo)
                                                              .SelectMany((symbols, token) => symbols)
                                                              .Where(t => t != null);

        IncrementalValueProvider<ImmutableArray<ITypeSymbol>> result = types.Collect();
        context.RegisterSourceOutput(result, Generate);
    }
}