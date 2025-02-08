using DotUML.CLI.Diagram;
using DotUML.CLI.Text;

namespace DotUML.CLI.Mermaid;

public class ClassDiagramGenerator
{
    public string GenerateDiagram(Namespaces namespaces)
    {
        var diagram = new IndentedStringBuilder();
        diagram.AppendLine("```mermaid");
        diagram.AppendLine("classDiagram");

        foreach (var ns in namespaces)
        {
            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.AppendLine($"namespace {ns.Name} {{");
                diagram.IncreaseIndent();
            }

            foreach (var obj in ns.ObjectInfos)
            {
                diagram.Append(obj.GetObjectRepresentation());
            }

            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.DecreaseIndent();
                diagram.AppendLine("}");
            }
        }

        foreach (var ns in namespaces)
        {
            foreach (var obj in ns.ObjectInfos.OfType<IHaveRelationships>())
            {
                diagram.Append(obj.GetRelationshipRepresentation());
            }
        }

        diagram.AppendLine("```");
        return diagram.ToString();
    }

    public void WriteToReadme(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        Console.WriteLine($"Mermaid UML diagram written to {outputPath}");
    }
}
