using System.Text;

namespace DotUML.CLI.Models;

public record TypeInfo(string Name)
{
    public string SanitizedName => Name.Replace('<', '~').Replace('>', '~');

    public bool IsList => Name.StartsWith("List<");

    public bool IsPrimitive => Name switch
    {
        "int" or "long" or "short" or "byte" or "float" or "double" or "decimal" or "bool" or "char" or "string" => true,
        _ => false
    };

    public static explicit operator TypeInfo(string name) => new TypeInfo(name);
}

public enum PropertyRelationship
{
    Aggregation,
    Composition,
    None
}

public record PropertyInfo(string Name, string Visibility, TypeInfo Type)
{
    public string GetDiagramRepresentation() => $"        {DiagramHelpers.GetVisibilityCharacter(Visibility)}{Name} : {Type.SanitizedName}\n";

    public PropertyRelationship Relationship => Type switch
    {
        { IsList: true } => PropertyRelationship.Composition,
        { IsPrimitive: false } => PropertyRelationship.Aggregation,
        _ => PropertyRelationship.None
    };

    public string GetRelationshipRepresentation(string objectName) => Relationship switch
    {
        PropertyRelationship.Aggregation => $"    {Type.SanitizedName} --o {Name}\n",
        PropertyRelationship.Composition => $"    {Type.SanitizedName} --* {Name}\n",
        _ => string.Empty
    };
}

public record MethodArgumentInfo(string Name, TypeInfo Type)
{
    public string GetDiagramRepresentation() => $"{Type.SanitizedName} {Name}";
}

public record MethodInfo(string Name, string Visibility, TypeInfo ReturnType)
{
    private readonly List<MethodArgumentInfo> _arguments = new();
    public void AddArgument(MethodArgumentInfo argument) => _arguments.Add(argument);

    private string GetArguments() => string.Join(", ", _arguments.Select(a => a.GetDiagramRepresentation()));

    public string GetDiagramRepresentation()
    {
        var returnType = ReturnType.Name.Contains("void") ? string.Empty : $" : {ReturnType.SanitizedName}";
        return $"        {DiagramHelpers.GetVisibilityCharacter(Visibility)}{Name}({GetArguments()}){returnType}\n";
    }
}

public record DependencyInfo(TypeInfo Type);

public abstract record ObjectInfo(string Name)
{
    protected List<PropertyInfo> _properties = new();
    protected List<MethodInfo> _methods = new();

    public abstract string GetDiagramRepresentation();

    public void AddProperty(PropertyInfo property) => _properties.Add(property);
    public void AddMethod(MethodInfo method) => _methods.Add(method);
}

public record ClassInfo(string Name, string? BaseClass = "") : ObjectInfo(Name)
{
    private readonly List<DependencyInfo> _dependencies = new();

    public override string GetDiagramRepresentation()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    class {Name} {{");
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendJoin(string.Empty, _methods.Select(m => m.GetDiagramRepresentation()));
        sb.AppendLine("    }");
        sb.AppendJoin(string.Empty, _dependencies.Select(d => $"    {Name} ..> {d.Type.SanitizedName}\n"));
        if (!string.IsNullOrEmpty(BaseClass))
        {
            sb.AppendLine($"    {BaseClass} <|-- {Name}");
        }
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetRelationshipRepresentation(Name)));
        return sb.ToString();
    }

    internal void AddDependency(DependencyInfo dependencyInfo) => _dependencies.Add(dependencyInfo);
}

public record InterfaceInfo(string Name) : ObjectInfo(Name)
{
    public override string GetDiagramRepresentation()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    class {Name} {{");
        sb.AppendLine("        <<interface>>");
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendJoin(string.Empty, _methods.Select(m => m.GetDiagramRepresentation()));
        sb.AppendLine("    }");
        return sb.ToString();
    }
}
