using DotUML.CLI.Diagram;
using DotUML.CLI.Text;

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
            diagram.IncreaseIndent();
            ns.ObjectInfos.Select(o => o.GetObjectRepresentation()).ToList().ForEach(r => diagram.Append(r));
            diagram.DecreaseIndent();
            if (!string.IsNullOrEmpty(ns.Name))
            {
                diagram.DecreaseIndent();
                diagram.AppendLine("    }");
            }
            ns.ObjectInfos.Select(o => o.GetRelationshipRepresentation()).ToList().ForEach(r => diagram.Append(r));
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