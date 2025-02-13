using System.Text.Encodings.Web;
using System.Threading.Tasks;

using DotUML.CLI.Diagram;
using DotUML.CLI.Text;

using Microsoft.Extensions.Logging;

namespace DotUML.CLI.Mermaid;

public class HtmlDiagramGenerator : IGenerateMermaidDiagram
{
    private readonly ILogger<HtmlDiagramGenerator> _logger;

    public HtmlDiagramGenerator(ILogger<HtmlDiagramGenerator> logger)
    {
        _logger = logger;
    }

    public OutputType OutputType => OutputType.HTML;

    public string GenerateDiagram(Namespaces namespaces)
    {
        var diagram = new IndentedStringBuilder();
        diagram.AppendLine("<!DOCTYPE html>");
        diagram.AppendLine("<html lang=\"en\">");
        diagram.AppendLine("<head>");
        diagram.IncreaseIndent();
        diagram.AppendLine("<meta charset=\"UTF-8\">");
        diagram.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        diagram.AppendLine("<title>Mermaid Diagram</title>");
        diagram.AppendLine("<script type=\"module\">");
        diagram.AppendLine("import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';");
        diagram.AppendLine("mermaid.initialize({ startOnLoad: true });");
        diagram.AppendLine("</script>");
        diagram.DecreaseIndent();
        diagram.AppendLine("</head>");
        diagram.AppendLine("<body>");
        diagram.IncreaseIndent();
        diagram.AppendLine("<div class=\"mermaid\">");
        diagram.IncreaseIndent();
        diagram.AppendLine("classDiagram");
        diagram.Append(HtmlEncoder.Default.Encode(namespaces.GetUMLDiagram()));
        diagram.DecreaseIndent();
        diagram.AppendLine("</div>");
        diagram.DecreaseIndent();
        diagram.AppendLine("</body>");
        diagram.AppendLine("</html>");
        return diagram.ToString();
    }

    public async Task WriteToFile(string outputPath, string content)
    {
        await File.WriteAllTextAsync(outputPath, content);
        _logger.LogCritical($"Mermaid HTML diagram written to {outputPath}");
    }
}
