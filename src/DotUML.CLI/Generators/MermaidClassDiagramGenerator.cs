using System.Text;

using DotUML.CLI.Diagram;

namespace DotUML.CLI.Generators;

public class MermaidClassDiagramGenerator
{
    public string GenerateDiagram(Namespaces objectInfos)
    {
        var diagram = new StringBuilder();
        diagram.Append("```mermaid\n");
        diagram.Append("classDiagram\n");


        diagram.AppendJoin(string.Empty, objectInfos.Select(o =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"    namespace {o.Name} {{");
            sb.AppendJoin(string.Empty, o.ObjectInfos.Select(o => o.GetDiagramRepresentation()));
            sb.AppendLine("    }");
            return sb.ToString();
        }));

        diagram.Append("```");
        return diagram.ToString();
    }

    public void WriteToReadme(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        Console.WriteLine($"Mermaid UML diagram written to {outputPath}");
    }
}