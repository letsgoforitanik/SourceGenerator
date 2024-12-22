using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators;

[Generator]
public class FilterClassGenerator : IIncrementalGenerator
{
    private static HashSet<INamedTypeSymbol> symbolCache = new();
    
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

        var attributeSymbol = ctx.SemanticModel.Compilation.GetTypeByMetadataName("Generators.GenerateFilterAttribute");
        
        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attributeSymbol, attributeData.AttributeClass))
            {
                IEnumerable<string> includeAttributesList = [], excludeAttributesList = [];
                
                var args = attributeData.NamedArguments;

                if (args.Any())
                {
                    var keypair = args.First();
                    var keypairValue = keypair.Value.Value as string;

                    if (keypair.Key == "Include") includeAttributesList = keypairValue?.Split(',') ?? [];
                    if (keypair.Key == "Exclude") excludeAttributesList = keypairValue?.Split(',') ?? [];
                }
                
                var namespaceName = classSymbol.ContainingNamespace.ToString();
                var className = classSymbol.Name;
                var properties = GetProperties(classSymbol);

                return new ClassInfo(namespaceName, className, properties, includeAttributesList, excludeAttributesList);
                
            }
        }

        return null;

    }
    
    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol classSymbol)
    {
        var properties = classSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(member => member is  { Kind: SymbolKind.Property, DeclaredAccessibility: Accessibility.Public });
        
        return properties;
    }
    
    private static void Execute(SourceProductionContext ctx, ImmutableArray<ClassInfo?> classInfos)
    {
        foreach (var classInfo in classInfos)
        {
            if (classInfo is null) return;
            
            var namespaceName = classInfo.NamespaceName;
            var className = classInfo.ClassName;
            var fileName = $"{namespaceName}.{className}Filter.g.cs";

            var properties = classInfo.Properties;

            if (classInfo.IncludeAttributesList.Any())
            {
                properties = properties.Where(prop => classInfo.IncludeAttributesList.Contains(prop.Name));
            }

            if (classInfo.ExcludeAttributesList.Any())
            {
                properties = properties.Where(prop => !classInfo.ExcludeAttributesList.Contains(prop.Name));
            }
            
            
            var source = $$"""

                           #nullable enable

                           namespace {{ namespaceName }};

                           public class {{ className }}Filter 
                           {
                           {{ GenerateString(properties) }}
                           } 

                           #nullable restore

                           """;

           ctx.AddSource(fileName, source);
           
        }
        
    }
    
    private static string GenerateString(IEnumerable<IPropertySymbol> properties)
    {
        var stringBuilder = new StringBuilder();

        foreach (var prop in properties)
        {
            var propTypeName = prop.Type.ToDisplayString();
            var makeNullable = IsPropertyNullable(prop) ? "" : "?";
            stringBuilder.Append($"\tpublic {propTypeName}{makeNullable} {prop.Name} {{ get; set; }}\n");
        }

        return stringBuilder.ToString();

    }
    
    private static bool IsPropertyNullable(IPropertySymbol propertySymbol) 
    { 
        // reference type
        if (propertySymbol.NullableAnnotation == NullableAnnotation.Annotated) return true;

        // value type
        return propertySymbol.Type is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T };
    }

        
    private static void RegisterPostInitializationOutput(IncrementalGeneratorPostInitializationContext ctx)
    {
        var fileName = "GenerateFilterAttribute.g.cs";
        
        var source = """
                     #nullable enable
                     
                     namespace Generators;
                     
                     [AttributeUsage(AttributeTargets.Class)]
                     public class GenerateFilterAttribute : Attribute
                     {
                         public string? Exclude { get; set; }
                         public string? Include { get; set; }
                     }
                     
                     #nullable restore
                     
                     """;
        
        ctx.AddSource(fileName, source);
        
    }
}
