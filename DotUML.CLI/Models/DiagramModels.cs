using System.Text;

namespace DotUML.CLI.Models;

public record PropertyInfo(string Name, string Visibility, string Type)
{
    public string GetDiagramRepresentation() => $"        {Visibility}{Name} : {Type}";
}

public abstract record ObjectInfo(string Name)
{
    protected List<PropertyInfo> _properties = new();
    public abstract string GetDiagramRepresentation();

    public void AddProperty(PropertyInfo property) => _properties.Add(property);
}

public record ClassInfo(string Name, string? BaseClass = "") : ObjectInfo(Name)
{
    public override string GetDiagramRepresentation()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    class {Name} {{");
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendLine("    }"); if (string.IsNullOrEmpty(BaseClass))
        {
            return sb.ToString();
        }
        sb.AppendLine($"    {BaseClass} <|-- {Name}");
        return sb.ToString();
    }
};

public record InterfaceInfo(string Name) : ObjectInfo(Name)
{
    public override string GetDiagramRepresentation()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    class {Name} {{");
        sb.AppendLine("        <<interface>>");
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendLine("    }");
        return sb.ToString();
    }
}
