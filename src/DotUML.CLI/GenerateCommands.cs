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

    public const string DefaultOutputFileName = "diagramyyyyMMddHHmmss.md";
    /// <summary>
    /// Generate a class diagram from a solution and write it to a file.
    /// </summary>
    /// <param name="solution">-s, Solution to analyze and generate UML diagram for.</param>
    /// <param name="outputFile">-o, Target location for UML file output. If a location is not provided, then a filename including a timestamp will be created in the current directory.</param>
    /// <param name="format">-f, Output type for the diagram. Options: markdown, image, html</param>
    public async Task Generate(string solution, OutputType format = OutputType.Markdown, string? outputFile = DefaultOutputFileName)
    {
        if (string.IsNullOrEmpty(solution))
        {
            Console.WriteLine("Please provide a solution path.");
            return;
        }
        if (string.IsNullOrEmpty(outputFile) || outputFile == DefaultOutputFileName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var extension = format switch
            {
                OutputType.Markdown => "md",
                OutputType.Image => "png",
                OutputType.Html => "html",
                _ => throw new ArgumentException("Invalid output type.")
            };
            outputFile = $"diagram{timestamp}.{extension}";
        }
        var mermaidDiagramGenerator = _diagramGenerators.FirstOrDefault(g => g.OutputType == format);
        if (mermaidDiagramGenerator == null)
        {
            throw new ArgumentException("Invalid output type.");
        }
        var classes = await _classAnalyzer.ExtractClassesFromSolutionAsync(solution);
        var diagram = mermaidDiagramGenerator.GenerateDiagram(classes);
        await mermaidDiagramGenerator.WriteToFile(outputFile, diagram);
    }
}
