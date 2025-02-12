using System.Threading.Tasks;

using DotUML.CLI.Diagram;
using DotUML.CLI.Text;

using Microsoft.Extensions.Logging;

namespace DotUML.CLI.Mermaid;

public class MarkdownDiagramGenerator : IGenerateMermaidDiagram
{
    private readonly ILogger<MarkdownDiagramGenerator> _logger;

    public MarkdownDiagramGenerator(ILogger<MarkdownDiagramGenerator> logger)
    {
        _logger = logger;
    }

    public OutputType OutputType => OutputType.Markdown;

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

    public async Task WriteToFile(string outputPath, string content)
    {
        await File.WriteAllTextAsync(outputPath, content);
        _logger.LogCritical($"Mermaid UML diagram written to {outputPath}");
    }
}
