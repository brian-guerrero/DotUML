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

        diagram.Append(namespaces.GetUMLDiagram());

        diagram.DecreaseIndent();
        diagram.AppendLine("```");
        return diagram.ToString();
    }

    public void WriteToFile(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        _logger.LogCritical($"Mermaid UML diagram written to {outputPath}");
    }
}
