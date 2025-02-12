using DotUML.CLI.Analyzers;
using DotUML.CLI.Mermaid;

using Microsoft.Extensions.DependencyInjection;

namespace DotUML.CLI;

public partial class GenerateCommands
{
    private readonly ClassAnalyzer _classAnalyzer;
    private readonly IEnumerable<IGenerateMermaidDiagram> _diagramGenerators;

    public GenerateCommands(ClassAnalyzer classAnalyzer, IEnumerable<IGenerateMermaidDiagram> diagramGenerators)
    {
        _classAnalyzer = classAnalyzer;
        _diagramGenerators = diagramGenerators;
    }

    /// <summary>
    /// Generate a class diagram from a solution and write it to a file.
    /// </summary>
    /// <param name="solution">-s, Solution to analyze and generate UML diagram for.</param>
    /// <param name="outputFile">-o, Target location for UML file output.</param>
    /// <param name="outputType">-t, Output type for the diagram. Options: Markdown, Image</param>
    public async Task Generate(string solution, string? outputFile = "", OutputType outputType = OutputType.Image)
    {
        if (string.IsNullOrEmpty(solution))
        {
            Console.WriteLine("Please provide a solution path.");
            return;
        }
        if (string.IsNullOrEmpty(outputFile))
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var extension = outputType switch
            {
                OutputType.Markdown => "md",
                OutputType.Image => "png",
                _ => throw new ArgumentException("Invalid output type.")
            };
            outputFile = $"diagram{timestamp}.{extension}";
        }
        var mermaidDiagramGenerator = _diagramGenerators.FirstOrDefault(g => g.OutputType == outputType);
        if (mermaidDiagramGenerator == null)
        {
            throw new ArgumentException("Invalid output type.");
        }
        var classes = await _classAnalyzer.ExtractClassesFromSolutionAsync(solution);
        var diagram = mermaidDiagramGenerator.GenerateDiagram(classes);
        await mermaidDiagramGenerator.WriteToFile(outputFile, diagram);
    }
}
