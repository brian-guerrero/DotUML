using System.Text;

using DotUML.CLI.Diagram;

namespace DotUML.CLI.Mermaid;

public class ClassDiagramGenerator
{
    public string GenerateDiagram(Namespaces namespaces)
    {
        var diagram = new IndentedStringBuilder();
        diagram.Append("```mermaid\n");
        diagram.Append("classDiagram\n");


        foreach (var ns in namespaces)
        {
            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.IncreaseIndent();
                diagram.AppendLine($"namespace {ns.Name} {{");
            }
            diagram.AppendJoin(string.Empty, ns.ObjectInfos.Select(o => o.GetDiagramRepresentation()));
            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.DecreaseIndent();
                diagram.AppendLine("    }");
            }
        }

        diagram.Append("```");
        return diagram.ToString();
    }

    public void WriteToReadme(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        Console.WriteLine($"Mermaid UML diagram written to {outputPath}");
    }
}