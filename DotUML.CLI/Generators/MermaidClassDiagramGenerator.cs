using System.Text;

using DotUML.CLI.Models;

namespace DotUML.CLI.Generators;

public class MermaidClassDiagramGenerator
{
    public string GenerateDiagram(IEnumerable<ObjectInfo> objectInfos)
    {
        var diagram = new StringBuilder();
        diagram.AppendLine("```mermaid");
        diagram.AppendLine("classDiagram");

        diagram.AppendJoin(string.Empty, objectInfos.Select(o => o.GetDiagramRepresentation()));

        diagram.AppendLine("```");
        return diagram.ToString();
    }

    public void WriteToReadme(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        Console.WriteLine($"Mermaid UML diagram written to {outputPath}");
    }
}