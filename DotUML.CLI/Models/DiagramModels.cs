using System.Text;

namespace DotUML.CLI.Models;

public record PropertyInfo(string Name, string Visibility, string Type)
{
    private char VisibilityCharacter => Visibility switch
    {
        var v when v.Contains("public") => '+',
        var v when v.Contains("protected") => '#',
        var v when v.Contains("private") => '-',
        _ => '?'
    };
    public string GetDiagramRepresentation() => $"        {VisibilityCharacter}{Name} : {Type}\n";
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
