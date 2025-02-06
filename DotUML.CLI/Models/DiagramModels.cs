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

public record MethodArgumentInfo(string Name, string Type)
{
    public string GetDiagramRepresentation() => $"{Type} {Name}";
}

public record MethodInfo(string Name, string Visibility, string ReturnType)
{
    private List<MethodArgumentInfo> _arguments = new();
    public void AddArgument(MethodArgumentInfo argument) => _arguments.Add(argument);

    private string GetArguments() => string.Join(", ", _arguments.Select(a => a.GetDiagramRepresentation()));
    private char VisibilityCharacter => Visibility switch
    {
        var v when v.Contains("public") => '+',
        var v when v.Contains("protected") => '#',
        var v when v.Contains("private") => '-',
        _ => '?'
    };
    public string GetDiagramRepresentation()
    {
        if (ReturnType.Contains("void"))
        {
            return $"        {VisibilityCharacter}{Name}({GetArguments()})\n";
        }
        return $"        {VisibilityCharacter}{Name}({GetArguments()}) : {ReturnType}\n";
    }
}

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
    public override string GetDiagramRepresentation()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    class {Name} {{");
        sb.AppendJoin(string.Empty, _properties.Select(p => p.GetDiagramRepresentation()));
        sb.AppendJoin(string.Empty, _methods.Select(p => p.GetDiagramRepresentation()));
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
        sb.AppendJoin(string.Empty, _methods.Select(p => p.GetDiagramRepresentation()));
        sb.AppendLine("    }");
        return sb.ToString();
    }
}
