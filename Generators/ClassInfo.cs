namespace Generators;

public class ClassInfo 
{
    public ClassInfo(string namespaceName, string className, IEnumerable<string> propertyNames)
    {
        NamespaceName = namespaceName;
        ClassName = className;
        PropertyNames = propertyNames;
    }
    
    public string NamespaceName { get; }
    public string ClassName { get; }
    public IEnumerable<string> PropertyNames { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not ClassInfo classInfo) return false;

        var namespaceNameMatches = this.NamespaceName == classInfo.NamespaceName;
        var classNameMatches = this.ClassName == classInfo.ClassName;
        var propertyNamesMatches = this.PropertyNames.SequenceEqual(classInfo.PropertyNames);

        return namespaceNameMatches && classNameMatches && propertyNamesMatches;
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 31 + NamespaceName.GetHashCode();
        hash = hash * 31 + ClassName.GetHashCode();

        hash = PropertyNames.Select(propertyName => hash * 31 + propertyName.GetHashCode()).Sum();
        
        return hash;
    }
}