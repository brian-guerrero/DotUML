using DotUML.CLI.Analyzers;
using DotUML.CLI.Mermaid;

namespace DotUML.CLI;

public class GenerateCommands
{
    private readonly ClassAnalyzer _classAnalyzer;
    private readonly ClassDiagramGenerator _classDiagramGenerator;

    public GenerateCommands(ClassAnalyzer classAnalyzer, ClassDiagramGenerator classDiagramGenerator)
    {
        _classAnalyzer = classAnalyzer;
        _classDiagramGenerator = classDiagramGenerator;
    }

    /// <summary>
    /// Generate a class diagram from a solution and write it to a file.
    /// </summary>
    /// <param name="solution">-s, Solution to analyze and generate UML diagram for.</param>
    /// <param name="outputFile">-o, Target location for UML file output.</param>
    public async Task Generate(string solution, string? outputFile = "")
    {
        if (string.IsNullOrEmpty(solution))
        {
            Console.WriteLine("Please provide a solution path.");
            return;
        }
        if (string.IsNullOrEmpty(outputFile))
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            outputFile = $"diagram{timestamp}.md";
        }
        var classes = await _classAnalyzer.ExtractClassesFromSolutionAsync(solution);
        var diagram = _classDiagramGenerator.GenerateDiagram(classes);
        _classDiagramGenerator.WriteToFile(outputFile, diagram);
    }
}
