using DotUML.CLI.Diagram;
using DotUML.CLI.Text;

using Microsoft.Extensions.Logging;

namespace DotUML.CLI.Mermaid;

public class ClassDiagramGenerator
{
    private readonly ILogger<ClassDiagramGenerator> _logger;

    public ClassDiagramGenerator(ILogger<ClassDiagramGenerator> logger)
    {
        _logger = logger;
    }

    public string GenerateDiagram(Namespaces namespaces)
    {
        var diagram = new IndentedStringBuilder();
        diagram.AppendLine("```mermaid");
        diagram.AppendLine("classDiagram");
        diagram.IncreaseIndent();

        foreach (var ns in namespaces)
        {
            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.AppendLine($"namespace {ns.Name} {{");
                diagram.IncreaseIndent();
            }

            foreach (var obj in ns.ObjectInfos)
            {
                diagram.Append(obj.GetObjectRepresentation().Trim());
            }

            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.DecreaseIndent();
                diagram.AppendLine("}");
            }
        }


        foreach (var obj in namespaces.SelectMany(ns => ns.ObjectInfos.OfType<IHaveRelationships>()))
        {
            if (!string.IsNullOrWhiteSpace(obj.GetRelationshipRepresentation()))
            {
                diagram.Append(obj.GetRelationshipRepresentation().Trim());
            }
        }

        diagram.DecreaseIndent();
        diagram.AppendLine("```");
        return diagram.ToString();
    }

    public void WriteToReadme(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        _logger.LogCritical($"Mermaid UML diagram written to {outputPath}");
    }
}
