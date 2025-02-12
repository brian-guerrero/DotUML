using DotUML.CLI.Diagram;

using static DotUML.CLI.GenerateCommands;


namespace DotUML.CLI.Mermaid;

public interface IGenerateMermaidDiagram
{
    string GenerateDiagram(Namespaces namespaces);
    Task WriteToFile(string outputPath, string content);
    public OutputType OutputType { get; }
}
