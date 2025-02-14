using System.Collections;

using DotUML.CLI.Text;

namespace DotUML.CLI.Diagram;

public record TypeInfo(string Name)
{
    public string SanitizedName => Name.Replace('<', '~').Replace('>', '~');
}

public record CreatedType(string Name) : TypeInfo(Name);

public record PrimitiveType(string Name) : TypeInfo(Name);

public record AggregateType(string Name, TypeInfo AggregatedType) : TypeInfo(Name);

public record NullableType(string Name, TypeInfo ElementType) : TypeInfo(Name);

public enum PropertyRelationship
{
    Aggregation,
    Composition,
    None
}

public record PropertyInfo(string Name, string Visibility, TypeInfo Type)
{
    public string GetDiagramRepresentation() => $"{Helpers.GetVisibilityCharacter(Visibility)}{Name} : {Type.SanitizedName}";

    public string GetRelationshipRepresentation(string parentName)
    {
        return Type switch
        {
            AggregateType aggregateType when aggregateType.AggregatedType is not PrimitiveType => $"{parentName} o-- {aggregateType.AggregatedType.SanitizedName}",
            CreatedType => $"{parentName} --> {Type.SanitizedName}",
            NullableType nullableType when nullableType.ElementType is not PrimitiveType => $"{parentName} --> \"0..1\" {nullableType.ElementType.SanitizedName}",
            _ => string.Empty
        };
    }
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
    public string SanitizedName => Name.Replace('<', '~').Replace('>', '~');
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
        sb.AppendLine($"class {SanitizedName} {{");
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
        sb.AppendLine($"class {SanitizedName} {{");
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
        sb.AppendJoin(string.Empty, _dependencies.Select(d => $"{SanitizedName} ..> {d.Type.SanitizedName}"));
        sb.AppendJoin(string.Empty, _interfaces.Select(i => $"{i} <|.. {SanitizedName}"));
        if (!string.IsNullOrEmpty(BaseClass))
        {
            sb.AppendLine($"{BaseClass} <|-- {SanitizedName}");
        }
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetRelationshipRepresentation(SanitizedName)));
        return sb.ToString();
    }

    internal void AddDependency(DependencyInfo dependencyInfo)
    {
        if (dependencyInfo.Type is PrimitiveType) return;
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
        sb.AppendLine($"class {SanitizedName} {{");
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
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetRelationshipRepresentation(SanitizedName)));
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

        foreach (var relationship in this.SelectMany(ns => ns.ObjectInfos.OfType<IHaveRelationships>().Select(s => s.GetRelationshipRepresentation()).Distinct().Where(s => !string.IsNullOrWhiteSpace(s))))
        {
            sb.Append(relationship.Trim());
        }
        return sb.ToString();
    }

    public string Key { get; }

    public IEnumerator<NamespaceInfo> GetEnumerator() => _namespaces.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private readonly HashSet<NamespaceInfo> _namespaces = new();

    public void Add(NamespaceInfo namespaceInfo) => _namespaces.Add(namespaceInfo);
}
