using System.Collections;

using DotUML.CLI.Text;

namespace DotUML.CLI.Diagram;

public record TypeInfo(string Name)
{
    public string SanitizedName => Name.Replace('<', '~').Replace('>', '~');

    public bool IsList => Name.StartsWith("List<");

    public bool IsPrimitive => Name.Replace("?", string.Empty) switch
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
    public string GetDiagramRepresentation() => $"{Helpers.GetVisibilityCharacter(Visibility)}{Name} : {Type.SanitizedName}";

    public PropertyRelationship Relationship => Type switch
    {
        { IsList: true } => PropertyRelationship.Composition,
        { IsPrimitive: false } => PropertyRelationship.Aggregation,
        _ => PropertyRelationship.None
    };

    public string GetRelationshipRepresentation(string objectName) => Relationship switch
    {
        PropertyRelationship.Aggregation => $"{Type.SanitizedName} --o {objectName}",
        PropertyRelationship.Composition => $"{Type.SanitizedName} --* {objectName}",
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
        return $"{Helpers.GetVisibilityCharacter(Visibility)}{Name}({GetArguments()}){returnType}";
    }
}

public record DependencyInfo(TypeInfo Type);

public interface IHaveRelationships
{
    public string GetRelationshipRepresentation();
}

public abstract record ObjectInfo(string Name)
{
    protected List<PropertyInfo> _properties = new();
    protected List<MethodInfo> _methods = new();
    public abstract string GetObjectRepresentation();
    public void AddProperty(PropertyInfo property) => _properties.Add(property);
    public void AddMethod(MethodInfo method) => _methods.Add(method);
}

public record EnumInfo(string Name) : ObjectInfo(Name)
{
    private readonly List<string> _values = new();

    public override string GetObjectRepresentation()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine($"class {Name} {{");
        sb.IncreaseIndent();
        sb.AppendLine("<<enumeration>>");
        sb.AppendJoin("", _values.Select(p => p));
        sb.DecreaseIndent();
        sb.AppendLine("}");
        return sb.ToString();
    }

    public void AddValue(string value) => _values.Add(value);
}

public record ClassInfo(string Name) : ObjectInfo(Name), IHaveRelationships
{
    private string? BaseClass { get; set; }
    private readonly List<DependencyInfo> _dependencies = new();
    private readonly List<string> _interfaces = new();

    public override string GetObjectRepresentation()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine($"class {Name} {{");
        sb.IncreaseIndent();
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendJoin(string.Empty, _methods.Select(m => m.GetDiagramRepresentation()));
        sb.DecreaseIndent();
        sb.AppendLine("}");
        return sb.ToString();
    }

    public string GetRelationshipRepresentation()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendJoin(string.Empty, _dependencies.Select(d => $"{Name} ..> {d.Type.SanitizedName}"));
        sb.AppendJoin(string.Empty, _interfaces.Select(i => $"{i} <|.. {Name}"));
        if (!string.IsNullOrEmpty(BaseClass))
        {
            sb.AppendLine($"{BaseClass} <|-- {Name}");
        }
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetRelationshipRepresentation(Name)));
        return sb.ToString();
    }

    internal void AddDependency(DependencyInfo dependencyInfo)
    {
        if (dependencyInfo.Type.IsPrimitive) return;
        _dependencies.Add(dependencyInfo);
    }

    internal void Implements(string interfaceName)
    {
        _interfaces.Add(interfaceName);
    }

    internal void Inherits(string baseClassName)
    {
        BaseClass = baseClassName;
    }
}

public record InterfaceInfo(string Name) : ObjectInfo(Name), IHaveRelationships
{
    public override string GetObjectRepresentation()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine($"class {Name} {{");
        sb.IncreaseIndent();
        sb.AppendLine("<<interface>>");
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendJoin(string.Empty, _methods.Select(m => m.GetDiagramRepresentation()));
        sb.DecreaseIndent();
        sb.AppendLine("}");
        return sb.ToString();
    }

    public string GetRelationshipRepresentation()
    {
        var sb = new IndentedStringBuilder();
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetRelationshipRepresentation(Name)));
        return sb.ToString();
    }
}

public record NamespaceInfo(string Name)
{
    public readonly HashSet<ObjectInfo> ObjectInfos = new();

    public void AddObjectInfo(params IEnumerable<ObjectInfo> objects)
    {
        foreach (var obj in objects)
        {
            ObjectInfos.Add(obj);
        }
    }
}

public class Namespaces : IGrouping<string, NamespaceInfo>
{

    public Namespaces(IEnumerable<NamespaceInfo> namespaces)
    {
        Key = namespaces.FirstOrDefault().Name;
        foreach (var namespaceInfo in namespaces)
        {
            Add(namespaceInfo);
        }
    }

    public string GetUMLDiagram()
    {
        var sb = new IndentedStringBuilder();
        foreach (var ns in this)
        {
            if (!string.IsNullOrEmpty(ns.Name))
            {
                sb.AppendLine($"namespace {ns.Name} {{");
                sb.IncreaseIndent();
            }

            foreach (var obj in ns.ObjectInfos)
            {
                sb.Append(obj.GetObjectRepresentation().Trim());
            }

            if (!string.IsNullOrEmpty(ns.Name))
            {
                sb.DecreaseIndent();
                sb.AppendLine("}");
            }
        }

        foreach (var obj in this.SelectMany(ns => ns.ObjectInfos.OfType<IHaveRelationships>()))
        {
            if (!string.IsNullOrWhiteSpace(obj.GetRelationshipRepresentation()))
            {
                sb.Append(obj.GetRelationshipRepresentation().Trim());
            }
        }
        return sb.ToString();
    }

    public string Key { get; }

    public IEnumerator<NamespaceInfo> GetEnumerator() => _namespaces.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private readonly HashSet<NamespaceInfo> _namespaces = new();

    public void Add(NamespaceInfo namespaceInfo) => _namespaces.Add(namespaceInfo);
}
