using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    private static HashSet<INamedTypeSymbol> symbolCache = new();

    private static Dictionary<string, int> dict = new();
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classInfos = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => GetSemanticTarget(ctx)
        );

        var collected = classInfos.Where(info => info is not null).Collect();
        
        context.RegisterSourceOutput(collected, static (ctx, source) => Execute(ctx, source!));
        context.RegisterPostInitializationOutput(static (ctx) => RegisterPostInitializationOutput(ctx));
        
    }
    
    private static ClassInfo? GetSemanticTarget(GeneratorSyntaxContext ctx)
    {
        
        var classDeclarationSyntax = ctx.Node as ClassDeclarationSyntax;
        
        var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax!)!;

        if (!symbolCache.Add(classSymbol)) return null;

        var attributeSymbol = ctx.SemanticModel.Compilation.GetTypeByMetadataName("Generators.GenerateToStringAttribute");
        
        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attributeSymbol, attributeData.AttributeClass))
            {
                var namespaceName = classSymbol.ContainingNamespace.ToString();
                var className = classSymbol.Name;
                var propertyNames = GetPropertyNames(classSymbol);

                return new ClassInfo(namespaceName, className, propertyNames);
            }
        }

        return null;

    }
    
    private static IEnumerable<string> GetPropertyNames(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetMembers()
            .Where(member => member is { Kind : SymbolKind.Property,  DeclaredAccessibility : Accessibility.Public } )
            .Select(prop => prop.Name);
    }
    
    private static void Execute(SourceProductionContext ctx, ImmutableArray<ClassInfo?> classInfos)
    {
        foreach (var classInfo in classInfos)
        {
            if (classInfo is null) return;
            
            var namespaceName = classInfo.NamespaceName;
            var className = classInfo.ClassName;
            var fileName = $"{namespaceName}.{className}.g.cs";

            if (!dict.ContainsKey(fileName))
            {
                dict.Add(fileName, 0);
            }

            dict[fileName]++;
            
            var source = $$"""
                           
                           namespace {{ namespaceName }};

                           partial class {{ className }} 
                           {
                               public override string ToString() 
                               {
                                   return {{ GenerateString(classInfo.PropertyNames) }};
                               }
                           } 

                           """;

           ctx.AddSource(fileName, source);
           
        }
        
    }
    
    private static string GenerateString(IEnumerable<string> propertyNames)
    {

        var stringBuilder = new StringBuilder("$\"");
        
        foreach (var propertyName in propertyNames)
        {
            stringBuilder.Append($@"{propertyName}:{{{propertyName}}}; ");
        }

        stringBuilder.Append("\"");

        stringBuilder.Remove(stringBuilder.Length - 3, 2);
        
        return stringBuilder.ToString();

    }

    private static void RegisterPostInitializationOutput(IncrementalGeneratorPostInitializationContext ctx)
    {
        var fileName = "GenerateToStringAttribute.g.cs";
        
        var source = """
                     namespace Generators;
                     
                     [AttributeUsage(AttributeTargets.Class)]
                     public class GenerateToStringAttribute : Attribute;
                     
                     """;
        
        ctx.AddSource(fileName, source);
        
    }
}
