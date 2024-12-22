using Microsoft.CodeAnalysis;

namespace Generators;

public class ClassInfo(string namespaceName, string className, IEnumerable<IPropertySymbol> properties, 
                IEnumerable<string> includeAttributesList, IEnumerable<string> excludeAttributesList)
{
    public string NamespaceName { get; } = namespaceName;
    public string ClassName { get; } = className;
    public IEnumerable<IPropertySymbol> Properties { get; } = properties;
    public IEnumerable<string> IncludeAttributesList { get; } = includeAttributesList;
    public IEnumerable<string> ExcludeAttributesList { get; } = excludeAttributesList;

    public override bool Equals(object? obj)
    {
        if (obj is not ClassInfo classInfo) return false;

        var namespaceNameMatches = this.NamespaceName == classInfo.NamespaceName;
        var classNameMatches = this.ClassName == classInfo.ClassName;
        var propertyNamesMatches = this.Properties.SequenceEqual(classInfo.Properties, SymbolEqualityComparer.Default);

        return namespaceNameMatches && classNameMatches && propertyNamesMatches;
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 31 + NamespaceName.GetHashCode();
        hash = hash * 31 + ClassName.GetHashCode();

        hash = Properties.Select(property => hash * 31 + property.GetHashCode()).Sum();
        
        return hash;
    }
}