using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => ShouldTransform(node),
            transform: static (ctx, _) => ctx.Node as ClassDeclarationSyntax
        );

        context.RegisterSourceOutput(classes, static (ctx, source) => Execute(ctx, source!));
        context.RegisterPostInitializationOutput(static (ctx) => RegisterPostInitializationOutput(ctx));
    }

    private static bool ShouldTransform(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax) return false;

        if (!classDeclarationSyntax.AttributeLists.Any()) return false;

        foreach (var attributeList in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                return attributeName is "GenerateToString" or "GenerateToStringAttribute";
            }
        }

        return false;
    }

    private static void Execute(SourceProductionContext ctx, ClassDeclarationSyntax classDeclarationSyntax)
    {
        
        var namespaceName = classDeclarationSyntax.Parent is BaseNamespaceDeclarationSyntax syntax
            ? syntax.Name.ToString()
            : string.Empty;
            
        var className = classDeclarationSyntax.Identifier.Text;
        var fileName = $"{namespaceName}.{className}.g.cs";

        var source = $$"""
                       namespace {{ namespaceName }};
                       
                       partial class {{ className }} 
                       {
                           public override string ToString() 
                           {
                               return {{ GenerateMembersInfo(classDeclarationSyntax) }};
                           }
                       } 
                       
                       """;

        ctx.AddSource(fileName, source);
        
    }
    
    private static string GenerateMembersInfo(ClassDeclarationSyntax classDeclarationSyntax)
    {

        var stringBuilder = new StringBuilder("$\"");
        
        foreach (var memberDeclarationSyntax in classDeclarationSyntax.Members)
        {
            if (memberDeclarationSyntax is not PropertyDeclarationSyntax propertyDeclarationSyntax) continue;
            if (!propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword)) continue;
                
            var propertyName = propertyDeclarationSyntax.Identifier.Text;
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
