using System.Threading.Tasks;

using DotUML.CLI.Analyzers;
using DotUML.CLI.Generators;
using DotUML.CLI.Models;

namespace DotUML.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run -- <solution-path>");
            return;
        }

        string solutionPath = args[0];
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "README.md");

        HashSet<ObjectInfo> classInfos;
        try
        {
            Console.WriteLine("Extracting classes...");
            classInfos = ClassAnalyzer.ExtractClassesFromSolution(solutionPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting classes: {ex.Message}\n{ex.StackTrace}");
            return;
        }
        Console.WriteLine("Generating UML...");
        string mermaidDiagram = MermaidClassDiagramGenerator.GenerateDiagram(classInfos);

        Console.WriteLine("Writing to README.md...");
        MermaidClassDiagramGenerator.WriteToReadme(outputPath, mermaidDiagram);

        Console.WriteLine("Done!");
    }
}